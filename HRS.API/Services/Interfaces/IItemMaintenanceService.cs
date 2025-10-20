using HRS.API.Contracts.DTOs.Maintenance;
using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.API.Services.Interfaces;

public interface IItemMaintenanceService
{
    Task<ItemMaintenanceResponseDto> GetAsync(string id);
    Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync();
    Task<ItemMaintenanceResponseDto> AddAsync(AddItemMaintenanceRequestDto request);
    Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(ItemMaintenanceRequestDto request);
}
