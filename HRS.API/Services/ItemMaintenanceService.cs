using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services.Interfaces;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRS.API.Services;

public class ItemMaintenanceService : IItemMaintenanceService
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _http;

    public ItemMaintenanceService(
        IItemMaintenanceRepository itemMaintenanceRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper)
    {
        _itemMaintenanceRepository = itemMaintenanceRepository;
        _http = httpContextAccessor;
        _mapper = mapper;
    }

    public async Task<ItemMaintenanceResponseDto> GetAsync(int id)
    {
        var record = await _itemMaintenanceRepository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException("Maintenance record not found.");
        return _mapper.Map<ItemMaintenanceResponseDto>(record);
    }

    public async Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync()
    {
        var records = await _itemMaintenanceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records);
    }

    public async Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(ItemMaintenanceRequestDto request)
    {
        var record = await _itemMaintenanceRepository.GetByIdAsync(request.Id)
                     ?? throw new KeyNotFoundException("Maintenance record not found.");

        if (record.Type != ItemMaintenanceType.Repair)
            throw new InvalidOperationException("Only 'Repair' maintenance can be marked as fixed.");

        if (request.QuantityFixed <= 0)
            throw new ArgumentException("Invalid quantity to fix.");

        // 从 JWT 读取操作者
        var userId =
            _http.HttpContext?.User?.FindFirst("sub")?.Value ??
            _http.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
            "system";

        // 标记为已修复
        record.Type = ItemMaintenanceType.Fixed;
        record.Remarks = request.Remarks ?? $"Marked as fixed on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
        record.UpdatedAt = DateTime.UtcNow;
        if (int.TryParse(userId, out var userIdInt))
            record.UpdatedById = userIdInt;
        else
            record.UpdatedById = null;

        _itemMaintenanceRepository.Update(record);
        await _itemMaintenanceRepository.SaveChangesAsync();

        return _mapper.Map<ItemMaintenanceResponseDto>(record);
    }
}
