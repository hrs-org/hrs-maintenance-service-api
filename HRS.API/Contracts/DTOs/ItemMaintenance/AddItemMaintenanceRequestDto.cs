using System.ComponentModel.DataAnnotations;
using HRS.Domain.Enums;

namespace HRS.API.Contracts.DTOs.Maintenance;

public sealed class AddItemMaintenanceRequestDto
{
    public int ItemId { get; set; }
    public int RentalOrderId { get; set; }        
    public ItemMaintenanceType Type { get; set; } 
    public int Quantity { get; set; }
    public int CreatedById { get; set; }
    public string? Remarks { get; set; }
}