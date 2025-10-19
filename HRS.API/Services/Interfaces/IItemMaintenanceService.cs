using HRS.API.Contracts.DTOs.Maintenance;
using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.API.Services.Interfaces;

public interface IItemMaintenanceService
{
    Task<ItemMaintenanceResponseDto> GetAsync(int id);
    Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync();
    Task<ItemMaintenanceResponseDto> AddAsync(int itemId, int quantity, string? remarks, int? rentalOrderId = null, ItemMaintenanceType? type = null, int? createdById = null, DateTime? createdAt = null, int? quantityFixed = null);
    Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(ItemMaintenanceRequestDto request);
}
