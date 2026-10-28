using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EzItam.Contracts;

// 화면과 API가 공유하는 데이터 모양입니다. 엔티티의 속성 이름을 그대로 따르므로
// 화면 쪽 Razor 뷰는 타입 이름만 바꿔도 대부분 그대로 동작합니다.
// 목록 응답에서는 컬렉션(Assignments 등)을 비워 두고, 상세 응답에서만 채웁니다.

public class CategoryRef
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DepreciationMonths { get; set; }
}

public class VendorRef
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DepartmentRef
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LocationRef
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string FullName { get; set; } = string.Empty;
}

public class AssetRef
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LicenseRef
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LicenseType LicenseType { get; set; }
}

public class EmployeeRef
{
    public int Id { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public DepartmentRef? Department { get; set; }
    public string DisplayName => $"{Name} ({EmployeeNo})";
}

public class AssetDto
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public CategoryRef? Category { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int? VendorId { get; set; }
    public VendorRef? Vendor { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiry { get; set; }
    public int? LocationId { get; set; }
    public LocationRef? Location { get; set; }
    public int? CurrentEmployeeId { get; set; }
    public EmployeeRef? CurrentEmployee { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public string? HostName { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsWarrantyExpired { get; set; }
    public bool IsWarrantyExpiringSoon { get; set; }
    public decimal? CurrentBookValue { get; set; }
    public List<AssignmentDto> Assignments { get; set; } = new();
    public List<MaintenanceDto> MaintenanceRecords { get; set; } = new();
    public List<LicenseAssignmentDto> LicenseAssignments { get; set; } = new();
}

public class AssignmentDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public AssetRef? Asset { get; set; }
    public int EmployeeId { get; set; }
    public EmployeeRef? Employee { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public string? AssignedBy { get; set; }
    public string? ReturnedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsOpen => ReturnedAt is null;
}

public class MaintenanceDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public AssetRef? Asset { get; set; }
    public MaintenanceType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    [DisplayFormat(DataFormatString = "{0:0}", ApplyFormatInEditMode = true)]
    public decimal? Cost { get; set; }
    public int? VendorId { get; set; }
    public VendorRef? Vendor { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public bool IsCompleted => CompletedAt.HasValue;
}

public class LicenseAssignmentDto
{
    public int Id { get; set; }
    public int SoftwareLicenseId { get; set; }
    public LicenseRef? SoftwareLicense { get; set; }
    public int? AssetId { get; set; }
    public AssetRef? Asset { get; set; }
    public int? EmployeeId { get; set; }
    public EmployeeRef? Employee { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string? AssignedBy { get; set; }
    public string? Notes { get; set; }
    public bool IsOpen => ReleasedAt is null;
}

public class SoftwareLicenseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public LicenseType LicenseType { get; set; }
    public string? LicenseKey { get; set; }
    public int TotalSeats { get; set; } = 1;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? VendorId { get; set; }
    public VendorRef? Vendor { get; set; }
    [DisplayFormat(DataFormatString = "{0:0}", ApplyFormatInEditMode = true)]
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public int UsedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool IsUnlimited { get; set; }
    public List<LicenseAssignmentDto> Assignments { get; set; } = new();
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Title { get; set; }
    public int? DepartmentId { get; set; }
    public DepartmentRef? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public string DisplayName => $"{Name} ({EmployeeNo})";
    public List<AssetDto> Assets { get; set; } = new();
    public List<AssignmentDto> Assignments { get; set; } = new();
    public List<LicenseAssignmentDto> LicenseAssignments { get; set; } = new();
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<EmployeeRef> Employees { get; set; } = new();
}

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public List<AssetRef> Assets { get; set; } = new();
    public List<LicenseRef> Licenses { get; set; } = new();
}

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public bool IsActive { get; set; } = true;
    public string FullName { get; set; } = string.Empty;
    public List<AssetRef> Assets { get; set; } = new();
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DepreciationMonths { get; set; } = 48;
    public List<AssetRef> Assets { get; set; } = new();
}

public class AuditDto
{
    public long Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityKey { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? Changes { get; set; }
    public string? UserName { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AgentSoftwareDto
{
    public long Id { get; set; }
    public int AgentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Version { get; set; }
    public string? InstallDate { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class AgentDto
{
    public int Id { get; set; }
    public string DeviceUuid { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? CpuName { get; set; }
    public int? CpuCores { get; set; }
    public long? TotalMemoryMb { get; set; }
    public string? OsName { get; set; }
    public string? OsVersion { get; set; }
    public string? OsBuild { get; set; }
    public string? LoggedOnUser { get; set; }
    public string? MacAddresses { get; set; }
    public string? IpAddress { get; set; }
    public bool? BitLockerOn { get; set; }
    public long? DiskTotalGb { get; set; }
    public string AgentVersion { get; set; } = string.Empty;
    public AgentStatus Status { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? LastInventoryAt { get; set; }
    public bool RebootPending { get; set; }
    public bool IsOnline { get; set; }
    public int? AssetId { get; set; }
    public AssetRef? Asset { get; set; }
    public int SoftwareCount { get; set; }
    public List<AgentSoftwareDto> Software { get; set; } = new();
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public bool IsLockedOut { get; set; }
}

public class UserFormDto
{
    public string? Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string Role { get; set; } = "Viewer";
}

public class DashboardDto
{
    public int TotalAssets { get; set; }
    public int AssignedAssets { get; set; }
    public int InStockAssets { get; set; }
    public int InRepairAssets { get; set; }
    public int RetiredAssets { get; set; }
    public decimal TotalPurchaseValue { get; set; }
    public int WarrantyExpiringCount { get; set; }
    public int WarrantyExpiredCount { get; set; }
    public int TotalLicenses { get; set; }
    public int LicensesExpiringCount { get; set; }
    public int LicensesOverAllocated { get; set; }
    public int OpenMaintenanceCount { get; set; }
    public List<KeyValuePair<string, int>> ByCategory { get; set; } = new();
    public List<KeyValuePair<AssetStatus, int>> ByStatus { get; set; } = new();
    public List<KeyValuePair<string, int>> ByDepartment { get; set; } = new();
    public List<AssetDto> WarrantyExpiring { get; set; } = new();
    public List<SoftwareLicenseDto> LicensesExpiring { get; set; } = new();
    public List<AssignmentDto> RecentAssignments { get; set; } = new();
    public List<MaintenanceDto> OpenMaintenance { get; set; } = new();
}

// ---- 쓰기 요청 ----
public sealed class CheckOutRequest
{
    public int EmployeeId { get; set; }
    public string? Notes { get; set; }
}

public sealed class CheckInRequest
{
    public AssetStatus NewStatus { get; set; } = AssetStatus.InStock;
    public int? LocationId { get; set; }
    public string? Notes { get; set; }
}

public sealed class ChangeStatusRequest
{
    public AssetStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}

public sealed class LicenseAssignRequest
{
    public int? EmployeeId { get; set; }
    public int? AssetId { get; set; }
    public string? Notes { get; set; }
}

public sealed class NextTagResponse
{
    public string Tag { get; init; } = string.Empty;
}

public sealed class AgentRingRequest
{
    public AgentRing Ring { get; set; } = AgentRing.Stable;
}

public sealed class AgentLinkRequest
{
    public int? AssetId { get; set; }
}

public class AssetLabelsDto
{
    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; }
    [JsonPropertyName("asset_tag")]
    public string AssetTag { get; set; } = string.Empty;
    [JsonPropertyName("asset_name")]
    public string AssetName { get; set; } = string.Empty;
    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; set; }
    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; set; }
}

public class InventoryDto : IValidatableObject
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Inventory Name field is required.")]
    public string Name { get; set; } = string.Empty;
    [Required]
    public DateOnly DateFrom { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    [Required]
    public DateOnly DateTo { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string? Notes { get; set; }
    public InventoryStatus InventoryStatus { get; set; }
    public int TotalAssetsCount { get; set; }
    public int ScannedAssetsCount { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        if (DateTo < today)
        {
            yield return new ValidationResult(
                "The inventory end date must be greater than today's date.",
                [nameof(DateTo)]
            );
        }

        if (DateTo < DateFrom)
        {
            yield return new ValidationResult(
                "The inventory end date must be greater than or equal to the start date.",
                [nameof(DateTo), nameof(DateFrom)]
            );
        }
    }
}

public class CreateInventoryDto
{
    public string Name { get; set; } = string.Empty;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
}

public class UpdateInventoryDto
{
    public string Name { get; set; } = string.Empty;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public string? Notes { get; set; }
    public InventoryStatus InventoryStatus { get; set; }
}

public class InventoryDetailDto
{
    public int Id { get; set; }
    public int IdAsset { get; set; }
    public string? AssetTag { get; set; }
    public string? CategoryName { get; set; }
    public string? AssetName { get; set; }
    public int IdLocation { get; set; }
    public string? LocationName { get; set; }
    public int IdEmployee { get; set; }
    public string? EmployeeName { get; set; }
    public AssetStatus Status { get; set; }
    public int IdLocationIV { get; set; }
    public string? LocationIVName { get; set; }
    public int IdEmployeeIV { get; set; }
    public string? EmployeeIVName { get; set; }
    public AssetStatus StatusIV { get; set; }
    public int ScanCount { get; set; }
    public string? Notes { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime UpdatedTime { get; set; }
}

public class InventoryDetailResponseDto : InventoryDto
{
    public List<InventoryDetailDto> Details { get; set; } = [];
}

public class ScanResultRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("idLocationIV")]
    public int? IdLocationIV { get; set; }
    [JsonPropertyName("statusIV")]
    public int StatusIV { get; set; }
    [JsonPropertyName("idEmployeeIV")]
    public int? IdEmployeeIV { get; set; }
    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

public class ScanResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ScanCount { get; set; }
}

public class HrEmployeeDto
{
    public string OrgCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? DeptCode { get; set; }
    public string? DeptName { get; set; }
    public string? DeptLevel { get; set; }
    public string? TitleCode { get; set; }
    public string? TitleName { get; set; }
    public string? PositionCode { get; set; }
    public string? PositionName { get; set; }
    public string? IsResigned { get; set; }
}
