using System.Security.Claims;
using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using MongoDB.Bson;

namespace HRS.Test.API.Services;

public class ItemMaintenanceServiceTests
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _http;
    private readonly ItemMaintenanceService _service;

    public ItemMaintenanceServiceTests()
    {
        _itemMaintenanceRepository = Substitute.For<IItemMaintenanceRepository>();
        _mapper = Substitute.For<IMapper>();
        _http = BuildHttpAccessor(userId: "10"); 
        _service = new ItemMaintenanceService(_itemMaintenanceRepository, _http, _mapper);
    }

    private static IHttpContextAccessor BuildHttpAccessor(string userId)
    {
        var ctx = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "test"))
        };
        return new HttpContextAccessor { HttpContext = ctx };
    }

    [Fact]
    public async Task GetAsync_WhenRecordExists_ReturnsMappedDto()
    {
        var record = new ItemMaintenance { _id = ObjectId.GenerateNewId().ToString(), Type = ItemMaintenanceType.Repair, Quantity = 2 };
        var dto = new ItemMaintenanceResponseDto { Id = 1 };

        _itemMaintenanceRepository.GetByIdAsync(1)
            .Returns(Task.FromResult<ItemMaintenance?>(record));
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);

        var result = await _service.GetAsync(1);

        result.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
    {
        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(null));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetAsync(1));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var records = new List<ItemMaintenance> { new() { _id = ObjectId.GenerateNewId().ToString() }, new() { _id = ObjectId.GenerateNewId().ToString() } };
        var dtos = new List<ItemMaintenanceResponseDto> { new() { Id = 1 }, new() { Id = 2 } };

        _itemMaintenanceRepository.GetAllAsync()
            .Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(records));
        _mapper.Map<IEnumerable<ItemMaintenanceResponseDto>>(records).Returns(dtos);

        var result = await _service.GetAllAsync();

        result.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenValid_UpdatesRecordAndReturnsDto()
    {
        var record = new ItemMaintenance { _id = ObjectId.GenerateNewId().ToString(), Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 3, Remarks = "Fixed" };
        var dto = new ItemMaintenanceResponseDto { Id = 1 };

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(record));
        _mapper.Map<ItemMaintenanceResponseDto>(record).Returns(dto);

        var result = await _service.MarkAsFixedAsync(request);

        record.Type.Should().Be(ItemMaintenanceType.Fixed);
        record.Remarks.Should().Be("Fixed");
        record.UpdatedById.Should().Be(10); 
        await _itemMaintenanceRepository.Received(1).SaveChangesAsync();
        _itemMaintenanceRepository.Received(1).Update(record);
        result.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
    {
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 1 };

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(null));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.MarkAsFixedAsync(request));
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenTypeNotRepair_ThrowsInvalidOperationException()
    {
        var record = new ItemMaintenance { _id = ObjectId.GenerateNewId().ToString(), Type = ItemMaintenanceType.Fixed, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 1 };

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(record));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MarkAsFixedAsync(request));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task MarkAsFixedAsync_WhenQuantityInvalid_ThrowsArgumentException(int quantityFixed)
    {
        var record = new ItemMaintenance { _id = ObjectId.GenerateNewId().ToString(), Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = quantityFixed };

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(record));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsFixedAsync(request));
    }
}
