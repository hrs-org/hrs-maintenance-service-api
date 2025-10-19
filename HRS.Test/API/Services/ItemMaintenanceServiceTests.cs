using AutoMapper;
using FluentAssertions;
using HRS.API.Contracts.DTOs.Maintenance;
using HRS.API.Services;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using HRS.Shared.Core.Interfaces;
using HRS.Shared.Core.Dtos;
using NSubstitute;
using MongoDB.Bson;

namespace HRS.Test.API.Services;

public class ItemMaintenanceServiceTests
{
    private readonly IItemMaintenanceRepository _itemMaintenanceRepository;
    private readonly IMapper _mapper;
    private readonly IUserContextService _userContextService;
    private readonly ItemMaintenanceService _service;

    public ItemMaintenanceServiceTests()
    {
        _itemMaintenanceRepository = Substitute.For<IItemMaintenanceRepository>();
        _mapper = Substitute.For<IMapper>();
        _userContextService = Substitute.For<IUserContextService>();
        
        // Setup mock user - provide all required properties
        var mockUserResult = Task.FromResult(new UserResponseDto 
        { 
            Id = 10, 
            FirstName = "Test", 
            LastName = "User",
            Email = "test@example.com",
            Role = "Employee"
        });
        _userContextService.GetUserAsync().Returns(mockUserResult);
        
        _service = new ItemMaintenanceService(_itemMaintenanceRepository, _userContextService, _mapper);
    }

    [Fact]
    public async Task GetAsync_WhenRecordExists_ReturnsMappedDto()
    {
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 2 };
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
        var records = new List<ItemMaintenance> { new() { Id = 1 }, new() { Id = 1 } };
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
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 5 };
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
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Fixed, Quantity = 5 };
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
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = quantityFixed };

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(record));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsFixedAsync(request));
    }

    [Fact]
    public async Task MarkAsFixedAsync_WhenQuantityExceedsTotal_ThrowsArgumentException()
    {
        var record = new ItemMaintenance { Id = 1, Type = ItemMaintenanceType.Repair, Quantity = 5 };
        var request = new ItemMaintenanceRequestDto { Id = 1, QuantityFixed = 10 }; // > record.Quantity

        _itemMaintenanceRepository.GetByIdAsync(Arg.Any<object>())
            .Returns(Task.FromResult<ItemMaintenance?>(record));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.MarkAsFixedAsync(request));
    }
}
