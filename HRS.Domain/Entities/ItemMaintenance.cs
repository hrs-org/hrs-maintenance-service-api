using HRS.Domain.Enums;

namespace HRS.Domain.Entities;

public class ItemMaintenance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public int ItemId { get; set; }
    public int? RentalOrderId { get; set; }
    public ItemMaintenanceType Type { get; set; } = ItemMaintenanceType.Repair;
    public int Quantity { get; set; }
    public int? QuantityFixed { get; set; }
    public string? Remarks { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
