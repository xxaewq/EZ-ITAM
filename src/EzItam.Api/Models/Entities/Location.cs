using System.ComponentModel.DataAnnotations;

namespace EzItam.Api.Models.Entities;

public class Location
{
    public int Id { get; set; }

    [Required(ErrorMessage = "위치명은 필수입니다.")]
    [StringLength(100)]
    [Display(Name = "위치명")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "건물")]
    public string? Building { get; set; }

    [StringLength(20)]
    [Display(Name = "층")]
    public string? Floor { get; set; }

    [StringLength(50)]
    [Display(Name = "호실")]
    public string? Room { get; set; }

    [Display(Name = "사용")]
    public bool IsActive { get; set; } = true;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    // Các collection cho nhân viên gốc và nhân viên kiểm kê (IV)
    public ICollection<InventoryDetail> InventoryDetails { get; set; } = [];
    public ICollection<InventoryDetail> InventoryDetailsIV { get; set; } = [];

    public string FullName => string.Join(" ", new[] { Building, Floor, Room, Name }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
