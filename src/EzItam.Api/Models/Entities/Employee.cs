using System.ComponentModel.DataAnnotations;

namespace EzItam.Api.Models.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required(ErrorMessage = "사번은 필수입니다.")]
    [StringLength(30)]
    [Display(Name = "사번")]
    public string EmployeeNo { get; set; } = string.Empty;

    [Required(ErrorMessage = "이름은 필수입니다.")]
    [StringLength(100)]
    [Display(Name = "이름")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "올바른 이메일 형식이 아닙니다.")]
    [StringLength(200)]
    [Display(Name = "이메일")]
    public string? Email { get; set; }

    [StringLength(50)]
    [Display(Name = "직급")]
    public string? Title { get; set; }

    [Display(Name = "부서")]
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }


    [Display(Name = "재직")]
    public bool IsActive { get; set; } = true;

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
    public ICollection<LicenseAssignment> LicenseAssignments { get; set; } = new List<LicenseAssignment>();
    public ICollection<InventoryDetail> InventoryDetails { get; set; } = [];
    public ICollection<InventoryDetail> InventoryDetailsIV { get; set; } = [];

    public string DisplayName => $"{Name} ({EmployeeNo})";
}
