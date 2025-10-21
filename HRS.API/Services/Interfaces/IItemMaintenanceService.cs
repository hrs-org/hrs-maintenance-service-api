using HRS.Shared.Core.Dtos;

namespace HRS.API.Services.Interfaces;

public interface IItemMaintenanceService
{
    Task<ItemMaintenanceResponseDto> GetAsync(string id);
    Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync();
    Task<ItemMaintenanceResponseDto> AddAsync(CreateItemMaintenanceRequestDto request);
    Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(FixItemMaintenanceRequestDto request);
    Task<IEnumerable<ItemMaintenanceResponseDto>> AddBatchAsync(CreateItemMaintenanceBatchRequestDto request);
    Task<IEnumerable<ItemMaintenanceResponseDto>> GetByItemIdAsync(string itemId);
}
