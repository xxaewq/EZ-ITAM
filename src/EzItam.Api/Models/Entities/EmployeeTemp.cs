using System.ComponentModel.DataAnnotations;

namespace EzItam.Api.Models.Entities
{
    public class EmployeeTemp
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string OrgCode { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string EmployeeNo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(30)]
        public string? DeptCode { get; set; }
        [StringLength(100)]
        public string? DeptName { get; set; }
        [StringLength(30)]
        public string? DeptLevel { get; set; }

        [StringLength(30)]
        public string? TitleCode { get; set; }
        [StringLength(100)]
        public string? TitleName { get; set; }

        [StringLength(30)]
        public string? PositionCode { get; set; }
        [StringLength(100)]
        public string? PositionName { get; set; }

        [Required]
        [StringLength(1)]
        public string IsResigned { get; set; } = string.Empty;

        [StringLength(100)]
        public string? DateValue1 { get; set; }
        [StringLength(100)]
        public string? DateValue2 { get; set; }
        [StringLength(100)]
        public string? DateValue3 { get; set; }
        [StringLength(100)]
        public string? DateValue4 { get; set; }
        [StringLength(100)]
        public string? DateValue5 { get; set; }
        [StringLength(100)]
        public string? DateValue6 { get; set; }
        [StringLength(100)]
        public string? DateValue7 { get; set; }
        [StringLength(100)]
        public string? DateValue8 { get; set; }
        [StringLength(100)]
        public string? DateValue9 { get; set; }
    }
}
