namespace EzItam.Api.Models.Entities
{
    public class InventoryDetail
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;

        
        public int IdAsset { get; set; }
        public Asset Asset { get; set; } = null!;

        //Current information
        public int? IdLocation { get; set; }
        public Location? Location { get; set; }
        public AssetStatus Status { get; set; }
        public int? IdEmployee { get; set; }
        public Employee? Employee { get; set; }

        //Inventory information
        public int? IdLocationIV { get; set; }
        public Location? LocationIV { get; set; }
        public AssetStatus AssetStatusIV { get; set; }
        public int? IdEmployeeIV { get; set; }
        public Employee? EmployeeIV { get; set; } = null!;

        public string? Notes { get; set; }
        public int ScanCount { get; set; } = 0;
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; } = DateTime.Now;

    }
}
