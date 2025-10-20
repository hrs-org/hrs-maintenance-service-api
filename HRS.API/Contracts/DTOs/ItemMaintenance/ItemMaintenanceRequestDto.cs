using System.ComponentModel.DataAnnotations;

namespace HRS.API.Contracts.DTOs.Maintenance;

public class ItemMaintenanceRequestDto
{
    [Required] public string Id { get; set; } = string.Empty;
    [Required] public int QuantityFixed { get; set; }
    public string? Remarks { get; set; }
}
