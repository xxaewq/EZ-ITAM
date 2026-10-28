using EzItam.Api.Configurations;
using EzItam.Api.ExternalService;
using EzItam.Api.Infrastructure.BackgroundJobs;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// EZITAM_PROFILE=Sqlite 이면 appsettings.Sqlite.json 을 추가로 읽습니다 (로컬 데모용).
var profile = Environment.GetEnvironmentVariable("EZITAM_PROFILE");
if (!string.IsNullOrWhiteSpace(profile))
    builder.Configuration.AddJsonFile($"appsettings.{profile}.json", optional: true, reloadOnChange: true);

// ---------- Hangfire configuaration ----------
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2; // Số lượng worker xử lý song song
});

// ---------- Database (API 만 DB에 접근합니다) ----------
var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default 설정이 없습니다.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3));
});

// ---------- Identity (계정 저장만 사용, 쿠키 로그인은 쓰지 않습니다) ----------
builder.Services.AddIdentityCore<IdentityUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// ---------- JWT 인증 (FE 와 에이전트는 Bearer 토큰으로 호출) ----------
var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key 설정이 없습니다.");
if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt:Key 는 32자 이상이어야 합니다.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = jwt["Issuer"],
            ValidateAudience = true, ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEdit", p => p.RequireRole(AppRoles.Admin, AppRoles.Manager));
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.Admin));
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

// ---------- Application services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HttpCurrentUserService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<HrService>();

builder.Services.AddSingleton<IUpdateCatalog>(_ => new UpdateCatalog(
    builder.Configuration["Updates:Root"] is { Length: > 0 } root ? root : Path.Combine(builder.Environment.ContentRootPath, "updates")));

builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "EZ-ITAM API", Version = "v1" }));

var app = builder.Build();

var culture = new CultureInfo("ko-KR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
});

// ---------- Database init ----------
if (app.Configuration.GetValue("Database:AutoMigrate", true))
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsSqlite())
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(scope.ServiceProvider, app.Configuration, logger);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}


// Cấu hình Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");
// Định nghĩa Recurring Job chạy vào 03:00 AM Chủ Nhật hàng tuần
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var jobSettings = configuration.GetSection(HangFireJobSettings.SectionName).Get<HangFireJobSettings>()
                      ?? new HangFireJobSettings();

    recurringJobManager.AddOrUpdate<EmployeeSyncBackgroundService>(
        jobSettings.HrSync.JobId,
        service => service.SyncEmployeeDataAsync(jobSettings.HrSync.OrgCode, CancellationToken.None),
        jobSettings.HrSync.CronExpression,
        new RecurringJobOptions {
        TimeZone = TimeZoneInfo.Local
        }
    );
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
