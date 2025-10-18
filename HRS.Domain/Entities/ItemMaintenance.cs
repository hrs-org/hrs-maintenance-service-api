using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRS.Domain.Enums;

namespace HRS.Domain.Entities;

[Table("ItemMaintenances")]
public class ItemMaintenance
{
    [Key] public int Id { get; set; }

    [Required] public int ItemId { get; set; }    
    public int? RentalOrderId { get; set; }       

    [Required] public ItemMaintenanceType Type { get; set; } = ItemMaintenanceType.Repair;

    [Required] public int Quantity { get; set; }
    public int? QuantityFixed { get; set; }

    [MaxLength(250)] public string? Remarks { get; set; }

    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? UpdatedById { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
