using HRS.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HRS.Domain.Entities;

public class ItemMaintenance
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    public required string ItemId { get; set; }
    public required string RentalOrderId { get; set; }
    public int StoreId { get; set; }
    public ItemMaintenanceType Type { get; set; } = ItemMaintenanceType.Repair;
    public int Quantity { get; set; }
    public int? QuantityFixed { get; set; }
    public string? Remarks { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
