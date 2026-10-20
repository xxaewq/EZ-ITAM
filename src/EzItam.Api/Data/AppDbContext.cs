using System.Text.Json;
using EzItam.Api.Models.Entities;
using EzItam.Api.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EzItam.Api.Data;

public class AppDbContext : IdentityDbContext
{
    private readonly ICurrentUserService? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<SoftwareLicense> SoftwareLicenses => Set<SoftwareLicense>();
    public DbSet<LicenseAssignment> LicenseAssignments => Set<LicenseAssignment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentSoftware> AgentSoftware => Set<AgentSoftware>();

    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<InventoryDetail> InventoryDetails => Set<InventoryDetail>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Asset>(e =>
        {
            e.ToTable("Assets");
            e.HasIndex(a => a.AssetTag).IsUnique();
            e.HasIndex(a => a.SerialNumber);
            e.HasIndex(a => a.Status);
            e.HasOne(a => a.Category).WithMany(c => c.Assets).HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.Vendor).WithMany(v => v.Assets).HasForeignKey(a => a.VendorId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.Location).WithMany(l => l.Assets).HasForeignKey(a => a.LocationId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(a => a.CurrentEmployee).WithMany(emp => emp.Assets).HasForeignKey(a => a.CurrentEmployeeId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AssetCategory>(e =>
        {
            e.ToTable("AssetCategories");
            e.HasIndex(c => c.Code).IsUnique();
        });

        builder.Entity<Location>(e => e.ToTable("Locations"));

        builder.Entity<Department>(e =>
        {
            e.ToTable("Departments");
            e.HasIndex(d => d.Code).IsUnique();
        });

        builder.Entity<Employee>(e =>
        {
            e.ToTable("Employees");
            e.HasIndex(emp => emp.EmployeeNo).IsUnique();
            e.HasOne(emp => emp.Department).WithMany(d => d.Employees).HasForeignKey(emp => emp.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Vendor>(e => e.ToTable("Vendors"));

        builder.Entity<AssetAssignment>(e =>
        {
            e.ToTable("AssetAssignments");
            e.HasIndex(a => new { a.AssetId, a.ReturnedAt });
            e.HasOne(a => a.Asset).WithMany(a => a.Assignments).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee).WithMany(emp => emp.Assignments).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MaintenanceRecord>(e =>
        {
            e.ToTable("MaintenanceRecords");
            e.HasOne(m => m.Asset).WithMany(a => a.MaintenanceRecords).HasForeignKey(m => m.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Vendor).WithMany().HasForeignKey(m => m.VendorId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SoftwareLicense>(e =>
        {
            e.ToTable("SoftwareLicenses");
            e.HasIndex(l => l.Name);
            e.HasOne(l => l.Vendor).WithMany(v => v.Licenses).HasForeignKey(l => l.VendorId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<LicenseAssignment>(e =>
        {
            e.ToTable("LicenseAssignments");
            e.HasIndex(a => new { a.SoftwareLicenseId, a.ReleasedAt });
            e.HasOne(a => a.SoftwareLicense).WithMany(l => l.Assignments).HasForeignKey(a => a.SoftwareLicenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Asset).WithMany(a => a.LicenseAssignments).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.Employee).WithMany(emp => emp.LicenseAssignments).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Agent>(e =>
        {
            e.ToTable("Agents");
            e.HasIndex(a => a.DeviceUuid).IsUnique();
            e.HasIndex(a => a.SerialNumber);
            e.HasIndex(a => a.HostName);
            e.HasIndex(a => a.TokenHash);
            e.HasOne(a => a.Asset).WithMany(x => x.Agents).HasForeignKey(a => a.AssetId).OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AgentSoftware>(e =>
        {
            e.ToTable("AgentSoftware");
            e.HasIndex(s => new { s.AgentId, s.Name });
            e.HasOne(s => s.Agent).WithMany(a => a.Software).HasForeignKey(s => s.AgentId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs");
            e.HasIndex(a => new { a.EntityName, a.EntityKey });
            e.HasIndex(a => a.Timestamp);
        });

        builder.Entity<Inventory>(e =>
        {
            e.ToTable("Inventories");
            e.Property(i => i.Name).IsRequired().HasMaxLength(50);
            e.Property(i => i.InventoryStatus).IsRequired().HasConversion<int>();
            e.HasIndex(i => i.Name).IsUnique();
            e.Property(i => i.DateFrom).IsRequired();
            e.Property(i => i.DateTo).IsRequired();
            e.Property(i => i.Notes).IsRequired(false).HasMaxLength(200);
        });

        builder.Entity<InventoryDetail>(e =>
        {
            e.ToTable("InventoryDetails");
            e.HasOne(d => d.Inventory).WithMany(i => i.InventoryDetails).HasForeignKey(d => d.InventoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(d => d.Asset).WithMany(a => a.InventoryDetails).HasForeignKey(d => d.IdAsset).OnDelete(DeleteBehavior.Cascade);
            e.Property(d => d.IdLocation).IsRequired(false);
            e.HasOne(d => d.Location).WithMany(l => l.InventoryDetails).HasForeignKey(d => d.IdLocation).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdEmployee).IsRequired(false);
            e.HasOne(d => d.Employee).WithMany(emp => emp.InventoryDetails).HasForeignKey(d => d.IdEmployee).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdLocationIV).IsRequired(false);
            e.HasOne(d => d.LocationIV).WithMany(l => l.InventoryDetailsIV).HasForeignKey(d => d.IdLocationIV).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.IdEmployeeIV).IsRequired(false);
            e.HasOne(d => d.EmployeeIV).WithMany(emp => emp.InventoryDetailsIV).HasForeignKey(d => d.IdEmployeeIV).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.Notes).IsRequired(false).HasMaxLength(200);
            e.Property(d => d.ScanCount).IsRequired().HasDefaultValue(0);
            e.HasIndex(d => new { d.InventoryId, d.IdAsset }).IsUnique();
            e.HasIndex(d => new { d.InventoryId, d.ScanCount });
        });


    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        var added = ApplyAuditing(out var now, out var user);
        var result = base.SaveChanges(acceptAllChangesOnSuccess);
        if (added.Count > 0)
        {
            AuditLogs.AddRange(added.Select(e => BuildCreatedLog(e, now, user)));
            base.SaveChanges(acceptAllChangesOnSuccess);
        }
        return result;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var added = ApplyAuditing(out var now, out var user);
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        if (added.Count > 0)
        {
            AuditLogs.AddRange(added.Select(e => BuildCreatedLog(e, now, user)));
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        return result;
    }

    private static readonly HashSet<string> AuditedEntities = new()
    {
        nameof(Asset), nameof(AssetCategory), nameof(Location), nameof(Department), nameof(Employee),
        nameof(Vendor), nameof(AssetAssignment), nameof(MaintenanceRecord), nameof(SoftwareLicense), nameof(LicenseAssignment),
        nameof(Inventory), nameof(InventoryDetail)
    };

    /// <summary>
    /// 수정/삭제 로그는 즉시 추가하고, 생성 로그는 키(IDENTITY)가 확정되는 저장 이후에 기록하기 위해 엔트리 목록을 돌려줍니다.
    /// </summary>
    private List<EntityEntry> ApplyAuditing(out DateTime now, out string user)
    {
        now = DateTime.Now;
        user = _currentUser?.UserName ?? "system";
        var added = new List<EntityEntry>();
        var logs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;
            var typeName = entry.Metadata.ClrType.Name;
            if (!AuditedEntities.Contains(typeName)) continue;

            StampTimestamps(entry, now, user);

            if (entry.State == EntityState.Added)
            {
                added.Add(entry);
                continue;
            }

            var action = entry.State == EntityState.Deleted ? AuditAction.Deleted : AuditAction.Updated;
            var changes = BuildChanges(entry, action);
            if (action == AuditAction.Updated && changes.Count == 0) continue;

            logs.Add(new AuditLog
            {
                EntityName = typeName,
                EntityKey = BuildKey(entry),
                Action = action,
                Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes),
                UserName = user,
                Timestamp = now
            });
        }

        if (logs.Count > 0) AuditLogs.AddRange(logs);
        return added;
    }

    private static AuditLog BuildCreatedLog(EntityEntry entry, DateTime now, string user)
    {
        var changes = BuildChanges(entry, AuditAction.Created);
        return new AuditLog
        {
            EntityName = entry.Metadata.ClrType.Name,
            EntityKey = BuildKey(entry),
            Action = AuditAction.Created,
            Changes = changes.Count == 0 ? null : JsonSerializer.Serialize(changes),
            UserName = user,
            Timestamp = now
        };
    }

    private static void StampTimestamps(EntityEntry entry, DateTime now, string user)
    {
        if (entry.State == EntityState.Added)
        {
            SetIfExists(entry, "CreatedAt", now);
            SetIfExists(entry, "CreatedBy", user);
        }
        if (entry.State is EntityState.Added or EntityState.Modified)
        {
            SetIfExists(entry, "UpdatedAt", now);
            SetIfExists(entry, "UpdatedBy", user);
        }
    }

    private static void SetIfExists(EntityEntry entry, string property, object value)
    {
        var prop = entry.Metadata.FindProperty(property);
        if (prop is null) return;
        entry.Property(property).CurrentValue = value;
    }

    private static string BuildKey(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null) return string.Empty;
        return string.Join(",", key.Properties.Select(p => entry.Property(p.Name).CurrentValue?.ToString() ?? ""));
    }

    private static Dictionary<string, object?> BuildChanges(EntityEntry entry, AuditAction action)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            var name = prop.Metadata.Name;
            if (name is "UpdatedAt" or "UpdatedBy" or "CreatedAt" or "CreatedBy") continue;
            switch (action)
            {
                case AuditAction.Created:
                    if (prop.CurrentValue is not null) result[name] = new { New = prop.CurrentValue };
                    break;
                case AuditAction.Deleted:
                    if (prop.OriginalValue is not null) result[name] = new { Old = prop.OriginalValue };
                    break;
                default:
                    if (prop.IsModified && !Equals(prop.OriginalValue, prop.CurrentValue))
                        result[name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
                    break;
            }
        }
        return result;
    }
}
