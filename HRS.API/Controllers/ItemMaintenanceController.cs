using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services.Interfaces;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Shared.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/item-maintenances")]
[Authorize(Roles = "Employee,Manager,Admin")]
public class ItemMaintenanceController : ControllerBase
{
    private readonly IItemMaintenanceService _itemMaintenanceService;

    public ItemMaintenanceController(IItemMaintenanceService itemMaintenanceService)
    {
        _itemMaintenanceService = itemMaintenanceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemMaintenanceResponseDto>>> GetAll()
    {
        var result = await _itemMaintenanceService.GetAllAsync();
        return Ok(ApiResponse<List<ItemMaintenanceResponseDto>>.OkResponse(result.ToList()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> GetById(int id)
    {
        var result = await _itemMaintenanceService.GetAsync(id);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result));
    }

    [HttpPost]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> Add([FromBody] AddRequest request)
    {
        var result = await _itemMaintenanceService.AddAsync(request.ItemId, request.Quantity, request.Remarks);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result, "Maintenance record added successfully"));
    }

    [HttpPost("{id:int}/fix")]
    public async Task<ActionResult<ItemMaintenanceResponseDto>> MarkAsFixed(int id, [FromBody] ItemMaintenanceRequestDto request)
    {
        request.Id = id;
        var result = await _itemMaintenanceService.MarkAsFixedAsync(request);
        return Ok(ApiResponse<ItemMaintenanceResponseDto>.OkResponse(result, "Item maintenance marked as fixed successfully"));
    }
}

public class AddRequest
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public string? Remarks { get; set; }
}
