using EzItam.Web.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;

namespace EzItam.Web.Api;

/// <summary>본사 화면이 API를 호출하는 단일 창구입니다. 모든 데이터는 여기서만 오갑니다.</summary>
public sealed class ItamApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;

    public ItamApiClient(HttpClient http) => _http = http;

    // ---------- 공통 ----------
    private async Task<T> Send<T>(HttpMethod method, string path, object? body = null)
    {
        using var resp = await SendRaw(method, path, body);
        return (await resp.Content.ReadFromJsonAsync<T>(Json)) ?? throw new ApiException(HttpStatusCode.InternalServerError, "빈 응답");
    }

    private async Task SendNoContent(HttpMethod method, string path, object? body = null)
    {
        using var resp = await SendRaw(method, path, body);
    }

    private async Task<HttpResponseMessage> SendRaw(HttpMethod method, string path, object? body)
    {
        using var req = new HttpRequestMessage(method, path);
        if (body is not null) req.Content = JsonContent.Create(body, options: Json);
        var resp = await _http.SendAsync(req);
        if (resp.IsSuccessStatusCode) return resp;

        var message = "요청을 처리하지 못했습니다.";
        try
        {
            var err = await resp.Content.ReadFromJsonAsync<ApiErrorBody>(Json);
            if (!string.IsNullOrWhiteSpace(err?.Message)) message = err.Message!;
        }
        catch (JsonException) { }
        resp.Dispose();
        throw new ApiException(resp.StatusCode, message);
    }

    private sealed class ApiErrorBody
    {
        public string? Message { get; set; }
    }

    /// <summary>객체의 값이 있는 속성만 쿼리 문자열로 만듭니다.</summary>
    public static string Query(string path, object? query)
    {
        if (query is null) return path;
        var dict = new Dictionary<string, string?>();
        foreach (var p in query.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var v = p.GetValue(query);
            if (v is null) continue;
            if (v is string s && s.Length == 0) continue;
            if (v is bool b && !b) continue;
            dict[ToCamel(p.Name)] = v is bool bb ? "true" : Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
        }
        return QueryHelpers.AddQueryString(path, dict);
    }

    private static string ToCamel(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];

    // ---------- 인증 ----------
    public async Task<TokenResponse> LoginAsync(LoginRequest req)
    {
        using var anon = new HttpRequestMessage(HttpMethod.Post, "api/v1/auth/login") { Content = JsonContent.Create(req, options: Json) };
        using var resp = await _http.SendAsync(anon);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadFromJsonAsync<ApiErrorBody>(Json);
            throw new ApiException(resp.StatusCode, err?.Message ?? "로그인에 실패했습니다.");
        }
        return (await resp.Content.ReadFromJsonAsync<TokenResponse>(Json))!;
    }

    public Task ChangePasswordAsync(ChangePasswordRequest req) => SendNoContent(HttpMethod.Post, "api/v1/auth/password", req);

    // ---------- 자산 ----------
    public Task<PageResult<AssetDto>> SearchAssetsAsync(AssetFilter f) => Send<PageResult<AssetDto>>(HttpMethod.Get, Query("api/v1/assets", f));
    public Task<AssetDto> GetAssetAsync(int id) => Send<AssetDto>(HttpMethod.Get, $"api/v1/assets/{id}");
    public Task<AssetDto> CreateAssetAsync(AssetDto dto) => Send<AssetDto>(HttpMethod.Post, "api/v1/assets", dto);
    public Task<AssetDto> UpdateAssetAsync(int id, AssetDto dto) => Send<AssetDto>(HttpMethod.Put, $"api/v1/assets/{id}", dto);
    public Task DeleteAssetAsync(int id) => SendNoContent(HttpMethod.Delete, $"api/v1/assets/{id}");
    public Task<AssignmentDto> CheckOutAsync(int id, CheckOutRequest r) => Send<AssignmentDto>(HttpMethod.Post, $"api/v1/assets/{id}/checkout", r);
    public Task<AssignmentDto> CheckInAsync(int id, CheckInRequest r) => Send<AssignmentDto>(HttpMethod.Post, $"api/v1/assets/{id}/checkin", r);
    public Task ChangeStatusAsync(int id, ChangeStatusRequest r) => SendNoContent(HttpMethod.Post, $"api/v1/assets/{id}/status", r);
    public async Task<string> NextTagAsync(int categoryId) => (await Send<NextTagResponse>(HttpMethod.Get, $"api/v1/assets/next-tag?categoryId={categoryId}")).Tag;

    public async Task<byte[]> ExportAssetsAsync(AssetFilter f)
    {
        using var resp = await SendRaw(HttpMethod.Get, Query("api/v1/assets/export", f), null);
        return await resp.Content.ReadAsByteArrayAsync();
    }

    // ---------- 유지보수 ----------
    public Task<List<MaintenanceDto>> ListMaintenanceAsync(bool openOnly, MaintenanceType? type)
        => Send<List<MaintenanceDto>>(HttpMethod.Get, Query("api/v1/maintenance", new { openOnly, type }));
    public Task<MaintenanceDto> GetMaintenanceAsync(int id) => Send<MaintenanceDto>(HttpMethod.Get, $"api/v1/maintenance/{id}");
    public Task<MaintenanceDto> CreateMaintenanceAsync(MaintenanceDto dto, bool setInRepair)
        => Send<MaintenanceDto>(HttpMethod.Post, Query("api/v1/maintenance", new { setInRepair }), dto);
    public Task<MaintenanceDto> UpdateMaintenanceAsync(int id, MaintenanceDto dto, bool restoreToStock)
        => Send<MaintenanceDto>(HttpMethod.Put, Query($"api/v1/maintenance/{id}", new { restoreToStock }), dto);
    public Task DeleteMaintenanceAsync(int id) => SendNoContent(HttpMethod.Delete, $"api/v1/maintenance/{id}");

    // ---------- 라이선스 ----------
    public Task<List<SoftwareLicenseDto>> ListLicensesAsync(string? search, bool expiringOnly)
        => Send<List<SoftwareLicenseDto>>(HttpMethod.Get, Query("api/v1/licenses", new { search, expiringOnly }));
    public Task<SoftwareLicenseDto> GetLicenseAsync(int id) => Send<SoftwareLicenseDto>(HttpMethod.Get, $"api/v1/licenses/{id}");
    public Task<SoftwareLicenseDto> CreateLicenseAsync(SoftwareLicenseDto dto) => Send<SoftwareLicenseDto>(HttpMethod.Post, "api/v1/licenses", dto);
    public Task<SoftwareLicenseDto> UpdateLicenseAsync(int id, SoftwareLicenseDto dto) => Send<SoftwareLicenseDto>(HttpMethod.Put, $"api/v1/licenses/{id}", dto);
    public Task DeleteLicenseAsync(int id) => SendNoContent(HttpMethod.Delete, $"api/v1/licenses/{id}");
    public Task<LicenseAssignmentDto> AssignLicenseAsync(int id, LicenseAssignRequest r) => Send<LicenseAssignmentDto>(HttpMethod.Post, $"api/v1/licenses/{id}/assignments", r);
    public Task ReleaseLicenseAssignmentAsync(int assignmentId) => SendNoContent(HttpMethod.Post, $"api/v1/licenses/assignments/{assignmentId}/release");

    // ---------- 기준정보·사원·부서 등 (같은 모양의 CRUD) ----------
    public Task<List<T>> ListAsync<T>(string resource, object? query = null) => Send<List<T>>(HttpMethod.Get, Query($"api/v1/{resource}", query));
    public Task<T> GetAsync<T>(string resource, int id) => Send<T>(HttpMethod.Get, $"api/v1/{resource}/{id}");
    public Task<T> CreateAsync<T>(string resource, T dto) => Send<T>(HttpMethod.Post, $"api/v1/{resource}", dto);
    public Task<T> UpdateAsync<T>(string resource, int id, T dto) => Send<T>(HttpMethod.Put, $"api/v1/{resource}/{id}", dto);
    public Task DeleteAsync(string resource, int id) => SendNoContent(HttpMethod.Delete, $"api/v1/{resource}/{id}");

    public Task<List<SelectOption>> OptionsAsync(string kind) => Send<List<SelectOption>>(HttpMethod.Get, $"api/v1/options/{kind}");

    // ---------- 감사 로그 ----------
    public Task<PageResult<AuditDto>> AuditAsync(string? entity, string? user, DateTime? from, DateTime? to, string? key, int page)
        => Send<PageResult<AuditDto>>(HttpMethod.Get, Query("api/v1/audit", new { entity, user, from, to, key, page }));
    public Task<List<string>> AuditEntitiesAsync() => Send<List<string>>(HttpMethod.Get, "api/v1/audit/entities");

    // ---------- 사용자 (문자열 Id) ----------
    public Task<List<UserDto>> ListUsersAsync() => Send<List<UserDto>>(HttpMethod.Get, "api/v1/users");
    public Task<UserDto> CreateUserAsync(UserFormDto f) => Send<UserDto>(HttpMethod.Post, "api/v1/users", f);
    public Task<UserDto> UpdateUserAsync(string id, UserFormDto f) => Send<UserDto>(HttpMethod.Put, $"api/v1/users/{id}", f);
    public Task DeleteUserAsync(string id) => SendNoContent(HttpMethod.Delete, $"api/v1/users/{id}");
    public Task UnlockUserAsync(string id) => SendNoContent(HttpMethod.Post, $"api/v1/users/{id}/unlock");

    // ---------- 에이전트 (관리자) ----------
    public Task<List<AgentDto>> ListAgentsAsync(AgentStatus? status, string? search)
        => Send<List<AgentDto>>(HttpMethod.Get, Query("api/v1/agents", new { status, search }));
    public Task<AgentDto> GetAgentAsync(int id) => Send<AgentDto>(HttpMethod.Get, $"api/v1/agents/{id}");
    public Task ApproveAgentAsync(int id) => SendNoContent(HttpMethod.Post, $"api/v1/agents/{id}/approve");
    public Task DisableAgentAsync(int id) => SendNoContent(HttpMethod.Post, $"api/v1/agents/{id}/disable");
    public Task LinkAgentAsync(int id, int? assetId) => SendNoContent(HttpMethod.Post, $"api/v1/agents/{id}/link", new AgentLinkRequest { AssetId = assetId });

    // ---------- 대시보드 ----------
    public Task<DashboardDto> DashboardAsync() => Send<DashboardDto>(HttpMethod.Get, "api/v1/dashboard");

    public async Task<List<AssetLabelViewModel>> ListAssetLabelAsync(string ids)
    {
        var fromQuery = Query("api/v1/assets/labels", new { ids });
        return await Send<List<AssetLabelViewModel>>(HttpMethod.Get, fromQuery);
    }

    public async Task<List<InventoryDto>> ListInventoryAsync() => await Send<List<InventoryDto>>(HttpMethod.Get, "api/v1/inventories");
    public async Task<InventoryDto> GetInventoryByIdAsync(int id) => await Send<InventoryDto>(HttpMethod.Get, $"api/v1/inventories/{id}");
    public async Task<bool> CreateInventoryAsync(InventoryDto dto) => await Send<bool>(HttpMethod.Post, "api/v1/inventories", dto);
    public async Task<bool> UpdateInventoryAsync(int id, InventoryDto dto) => await Send<bool>(HttpMethod.Put, $"api/v1/inventories/{id}", dto);
    public async Task<bool> DeleteInventoryAsync(int id) => await Send<bool>(HttpMethod.Delete, $"api/v1/inventories/{id}");
    public async Task<bool> GenInventoryBookAsync(int id) => await Send<bool>(HttpMethod.Post, $"api/v1/inventories/{id}/genbook");
    public async Task<InventoryDetailResponseDto?> GetInventoryDetailByInventoryIdAsync(int id) => await Send<InventoryDetailResponseDto?>(HttpMethod.Get, $"api/v1/inventories/{id}/details");
    public async Task<InventoryDetailDto?> GetAssetByInventoryIdAndTag(int id, string tag)
    {
        var fromQuery = Query($"api/v1/inventories/{id}/asset", new { tag });
        return await Send<InventoryDetailDto?>(HttpMethod.Get, fromQuery);
    }

    public async Task<bool> PostScanResult(ScanResultRequest req) => await Send<bool>(HttpMethod.Post, "api/v1/inventories/scan", req);
    public async Task<bool> PostDiff(int id) => await Send<bool>(HttpMethod.Post, $"api/v1/inventories/postdiff/{id}");


    public async Task<List<HrEmployeeDto>> GetListEmployeesByOrgCode(string orgCode) => await Send<List<HrEmployeeDto>>(HttpMethod.Get, $"api/v1/external/hr/employees?orgCode={orgCode}");

}
