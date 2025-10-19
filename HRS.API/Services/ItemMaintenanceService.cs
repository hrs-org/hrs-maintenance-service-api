using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Interfaces;
using HRS.Shared.Core.Dtos;

namespace HRS.API.Services;

public class ItemMaintenanceService : IItemMaintenanceService
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;

    public ItemMaintenanceService(
        IItemMaintenanceRepository itemMaintenanceRepository,
        IUserContextService userContextService,
        IMapper mapper)
    {
        _itemMaintenanceRepository = itemMaintenanceRepository;
        _userContextService = userContextService;
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

    public async Task<ItemMaintenanceResponseDto> AddAsync(int itemId, int quantity, string? remarks, int? rentalOrderId = null, ItemMaintenanceType? type = null, int? createdById = null, DateTime? createdAt = null, int? quantityFixed = null)
    {
        // 如果没有提供用户信息，获取当前用户
        if (createdById == null)
        {
            var user = await _userContextService.GetUserAsync();
            createdById = user.Id;
        }

        var maintenance = new ItemMaintenance
        {
            Id = itemId,
            ItemId = itemId,
            RentalOrderId = rentalOrderId ?? itemId, 
            Type = type ?? ItemMaintenanceType.Repair, 
            Quantity = quantity,
            QuantityFixed = quantityFixed ?? quantity, 
            CreatedById = createdById.Value,
            CreatedAt = createdAt ?? DateTime.UtcNow, 
            Remarks = remarks
        };

        await _itemMaintenanceRepository.AddAsync(maintenance);
        await _itemMaintenanceRepository.SaveChangesAsync();

        return _mapper.Map<ItemMaintenanceResponseDto>(maintenance);
    }

    public async Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(ItemMaintenanceRequestDto request)
{
    var user = await _userContextService.GetUserAsync();

    var record = await _itemMaintenanceRepository.GetByIdAsync(request.Id)
                 ?? throw new KeyNotFoundException("Maintenance record not found.");

    if (record.Type != ItemMaintenanceType.Repair)
        throw new InvalidOperationException("Only 'Repair' maintenance can be marked as fixed.");

    if (request.QuantityFixed <= 0 || request.QuantityFixed > record.Quantity)
        throw new ArgumentException("Invalid quantity to fix.");

    // Mark as fixed
    record.Type = ItemMaintenanceType.Fixed;
    record.QuantityFixed = request.QuantityFixed;
    record.Remarks = request.Remarks ?? $"Marked as fixed on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
    record.UpdatedAt = DateTime.UtcNow;
    record.UpdatedById = user.Id;

    _itemMaintenanceRepository.Update(record);
    await _itemMaintenanceRepository.SaveChangesAsync();

    return _mapper.Map<ItemMaintenanceResponseDto>(record);
}
}
