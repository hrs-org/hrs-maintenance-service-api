using AutoMapper;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Dtos;
using HRS.Shared.Core.Enums;
using HRS.Shared.Core.Interfaces;
using System.Net.Http;

namespace HRS.API.Services;

public class ItemMaintenanceService : IItemMaintenanceService
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly HttpClient _httpClient;

    public ItemMaintenanceService(
        IItemMaintenanceRepository itemMaintenanceRepository,
        IUserContextService userContextService,
        IMapper mapper
, IHttpClientFactory httpClientFactory)
    {
        _itemMaintenanceRepository = itemMaintenanceRepository;
        _userContextService = userContextService;
        _mapper = mapper;
        _httpClient = httpClientFactory.CreateClient("InventoryService");
    }

    public async Task<ItemMaintenanceResponseDto> GetAsync(string id)
    {
        var record = await _itemMaintenanceRepository.GetByIdAsync(id)
                     ?? throw new KeyNotFoundException("Maintenance record not found.");
        return _mapper.Map<ItemMaintenanceResponseDto>(record);
    }

    public async Task<ItemMaintenanceResponseDto?> GetByItemIdAsync(string itemId)
    {
        var records = await _itemMaintenanceRepository.GetByItemIdAsync(itemId);
        return _mapper.Map<ItemMaintenanceResponseDto>(records);
    }

    public async Task<IEnumerable<ItemMaintenanceResponseDto>> GetAllAsync()
    {
        var records = await _itemMaintenanceRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records);
    }

    public async Task<ItemMaintenanceResponseDto> AddAsync(CreateItemMaintenanceRequestDto request)
    {
        var userId = _userContextService.GetUserId();
        var storeId = _userContextService.GetStoreId();

        var maintenance = new ItemMaintenance
        {
            ItemId = request.ItemId,
            Type = ItemMaintenanceType.Repair,
            RentalOrderId = request.RentalOrderId,
            Quantity = request.Quantity,
            QuantityFixed = 0,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            Remarks = request.Remarks,
            StoreId = storeId
        };

        await _itemMaintenanceRepository.AddAsync(maintenance);

        return _mapper.Map<ItemMaintenanceResponseDto>(maintenance);
    }

    public async Task<ItemMaintenanceResponseDto> MarkAsFixedAsync(FixItemMaintenanceRequestDto request)
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

    public async Task<IEnumerable<ItemMaintenanceResponseDto>> AddBatchAsync(CreateItemMaintenanceBatchRequestDto request)
    {
        var user = await _userContextService.GetUserAsync();
        var created = new List<ItemMaintenance>();
        var adjustments = new List<(string itemId, int delta)>();

        // 1) insert maintenance records
        foreach (var e in request.Entries)
        {
            var m = new ItemMaintenance
            {
                ItemId = e.ItemId,
                Type = e.Type,
                RentalOrderId = e.RentalOrderId,
                Quantity = e.Quantity,
                QuantityFixed = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedById = user.Id,
                Remarks = e.Remarks
            };
            await _itemMaintenanceRepository.AddAsync(m);
            created.Add(m);
        }

        // 2) for Broken/Lost, call ItemService to adjust quantity
        foreach (var e in request.Entries.Where(x => x.Type == ItemMaintenanceType.Broken || x.Type == ItemMaintenanceType.Lost))
        {
            var delta = -Math.Abs(e.Quantity);
            var resp = await _httpClient.PutAsJsonAsync($"/api/items/{e.ItemId}/quantity?quantity={delta}", new { Delta = delta });
            if (!resp.IsSuccessStatusCode)
            {
                // compensation: revert applied adjustments and delete created records (best-effort)
                foreach (var adj in adjustments)
                {
                    await _httpClient.PutAsJsonAsync($"/api/items/{adj.itemId}/quantity?Delta={-adj.delta}", new { Delta = -adj.delta });
                }
                foreach (var m in created) _itemMaintenanceRepository.Remove(m);
                throw new InvalidOperationException("Failed to update item quantity during maintenance batch. Compensation attempted.");
            }
            adjustments.Add((e.ItemId, delta));
        }

        return _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(created);
    }
}
