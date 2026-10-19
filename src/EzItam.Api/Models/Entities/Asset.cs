using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EzItam.Api.Models.Entities;

public class Asset
{
    public int Id { get; set; }

    [Required(ErrorMessage = "자산번호는 필수입니다.")]
    [StringLength(30)]
    [Display(Name = "자산번호")]
    public string AssetTag { get; set; } = string.Empty;

    [Required(ErrorMessage = "자산명은 필수입니다.")]
    [StringLength(200)]
    [Display(Name = "자산명")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "분류")]
    public int CategoryId { get; set; }
    public AssetCategory? Category { get; set; }

    [Display(Name = "상태")]
    public AssetStatus Status { get; set; } = AssetStatus.InStock;

    [StringLength(100)]
    [Display(Name = "제조사")]
    public string? Manufacturer { get; set; }

    [StringLength(100)]
    [Display(Name = "모델명")]
    public string? Model { get; set; }

    [StringLength(100)]
    [Display(Name = "시리얼번호")]
    public string? SerialNumber { get; set; }

    [Display(Name = "공급업체")]
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    [Display(Name = "구매일")]
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "구매가격")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 999999999999)]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "보증만료일")]
    [DataType(DataType.Date)]
    public DateTime? WarrantyExpiry { get; set; }

    [Display(Name = "위치")]
    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    [Display(Name = "현재 사용자")]
    public int? CurrentEmployeeId { get; set; }
    public Employee? CurrentEmployee { get; set; }

    [StringLength(100)]
    [Display(Name = "IP 주소")]
    public string? IpAddress { get; set; }

    [StringLength(100)]
    [Display(Name = "MAC 주소")]
    public string? MacAddress { get; set; }

    [StringLength(100)]
    [Display(Name = "호스트명")]
    public string? HostName { get; set; }

    [StringLength(100)]
    [Display(Name = "운영체제")]
    public string? OperatingSystem { get; set; }


    [StringLength(2000)]
    [Display(Name = "비고")]
    public string? Notes { get; set; }

    [Display(Name = "등록일시")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "수정일시")]
    public DateTime UpdatedAt { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    public ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
    public ICollection<Agent> Agents { get; set; } = new List<Agent>();

    public ICollection<InventoryDetail> InventoryDetails { get; set; } = [];

    [NotMapped]
    public bool IsWarrantyExpired => WarrantyExpiry.HasValue && WarrantyExpiry.Value.Date < DateTime.Today;

    [NotMapped]
    public bool IsWarrantyExpiringSoon => WarrantyExpiry.HasValue
        && WarrantyExpiry.Value.Date >= DateTime.Today
        && WarrantyExpiry.Value.Date <= DateTime.Today.AddDays(30);

    /// <summary>정액법 기준 현재 장부가액</summary>
    public decimal? CurrentBookValue(DateTime? asOf = null)
    {
        if (!PurchasePrice.HasValue || !PurchaseDate.HasValue || Category is null) return null;
        var months = Category.DepreciationMonths;
        if (months <= 0) return PurchasePrice;
        var now = asOf ?? DateTime.Today;
        var elapsed = (now.Year - PurchaseDate.Value.Year) * 12 + now.Month - PurchaseDate.Value.Month;
        if (elapsed <= 0) return PurchasePrice;
        if (elapsed >= months) return 0m;
        return Math.Round(PurchasePrice.Value * (months - elapsed) / months, 0);
    }
}
