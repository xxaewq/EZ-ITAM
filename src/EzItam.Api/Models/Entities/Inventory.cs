using System.ComponentModel.DataAnnotations;

namespace EzItam.Api.Models.Entities
{
    public class Inventory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string? Notes { get; set; }

        public InventoryStatus InventoryStatus { get; set; } = InventoryStatus.Open;

        public ICollection<InventoryDetail> InventoryDetails { get; set; } = [];
    }

    
}
