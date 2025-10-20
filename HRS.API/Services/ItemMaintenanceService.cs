using AutoMapper;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Interfaces;
using HRS.Shared.Core.Dtos;
using Stripe.Forwarding;

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

    public async Task<ItemMaintenanceResponseDto> GetAsync(string id)
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

    public async Task<ItemMaintenanceResponseDto> AddAsync(AddItemMaintenanceRequestDto request)
    {
        var user = await _userContextService.GetUserAsync();

        var maintenance = new ItemMaintenance
        {
            ItemId = request.ItemId,
            Type = ItemMaintenanceType.Repair,     
            RentalOrderId = null,                  
            Quantity = request.Quantity,
            QuantityFixed = 0,
            CreatedAt = DateTime.UtcNow,
            Remarks = request.Remarks
        };

        await _itemMaintenanceRepository.AddAsync(maintenance);

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
        record.QuantityFixed += request.QuantityFixed;
        if (record.QuantityFixed == record.Quantity)
        {
            record.Type = ItemMaintenanceType.Fixed;
        }
        record.Remarks = request.Remarks ?? $"Marked as fixed on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedById = user.Id;

        _itemMaintenanceRepository.Update(record);

        return _mapper.Map<ItemMaintenanceResponseDto>(record);
    }
}
