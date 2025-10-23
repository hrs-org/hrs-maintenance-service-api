using HRS.API.Services.Interfaces;
using HRS.Shared.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/item-maintenances")]
[Authorize]
public class ItemMaintenanceController : ControllerBase
{
    private readonly IItemMaintenanceService _itemMaintenanceService;

    public ItemMaintenanceController(IItemMaintenanceService itemMaintenanceService)
    {
        _itemMaintenanceService = itemMaintenanceService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ItemMaintenanceResponseDto>>> GetAll()
    {
        var result = await _itemMaintenanceService.GetAllAsync();
        return Ok(ApiResponse<List<ItemMaintenanceResponseDto>>.OkResponse(result.ToList()));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> GetById(string id)
    {
        var result = await _itemMaintenanceService.GetAsync(id);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result));
    }

    [HttpGet("items/{itemId}")]
    public async Task<ActionResult<IEnumerable<ItemMaintenanceResponseDto>>> GetByItemId(string itemId)
    {
        var result = await _itemMaintenanceService.GetByItemIdAsync(itemId);
        return Ok(ApiResponse<IEnumerable<ItemMaintenanceResponseDto>>.OkResponse(result));
    }

    [HttpGet("items")]
    public async Task<ActionResult<IEnumerable<ItemMaintenanceResponseDto>>> GetByStoreId([FromQuery] int storeId)
    {
        var result = await _itemMaintenanceService.GetByStoreIdAsync(storeId);
        return Ok(ApiResponse<IEnumerable<ItemMaintenanceResponseDto>>.OkResponse(result));
    }

    [HttpGet("items/{itemId}/repair/quantity")]
    public async Task<ActionResult<int>> GetRepairByItemId(string itemId)
    {
        var result = await _itemMaintenanceService.GetRepairingQuantityAsync(itemId);
        return Ok(ApiResponse<int>.OkResponse(result));
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> Add([FromBody] CreateItemMaintenanceRequestDto request)
    {
        var result = await _itemMaintenanceService.AddAsync(request);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result, "Maintenance record added successfully"));
    }

    [HttpPost("{id}/fix")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> MarkAsFixed(string id, [FromBody] FixItemMaintenanceRequestDto request)
    {
        request.Id = id;
        var result = await _itemMaintenanceService.MarkAsFixedAsync(request);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result, "Maintenance status updated successfully"));
    }

    [HttpPost("batch")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<IEnumerable<ItemMaintenanceResponseDto>>> AddBatch([FromBody] CreateItemMaintenanceBatchRequestDto request)
    {
        var result = await _itemMaintenanceService.AddBatchAsync(request);
        return Ok(ApiResponse<IEnumerable<ItemMaintenanceResponseDto>>.OkResponse(result, "Maintenance records added successfully"));
    }
}
