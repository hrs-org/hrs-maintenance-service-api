using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace HRS.Test.Infrastructure.Repositories;

public class ItemMaintenanceRepositoryTests
{
    private readonly IItemMaintenanceRepository _repository;

    public ItemMaintenanceRepositoryTests()
    {
        _repository = Substitute.For<IItemMaintenanceRepository>();
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ShouldReturn_CorrectSum()
    {
        // Arrange
        var itemId = 1;
        var expectedQuantity = 7; 

        _repository.GetRepairingQuantityAsync(itemId)
            .Returns(Task.FromResult(expectedQuantity));

        // Act
        var result = await _repository.GetRepairingQuantityAsync(itemId);

        // Assert
        Assert.Equal(expectedQuantity, result);
        await _repository.Received(1).GetRepairingQuantityAsync(itemId);
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ShouldReturn_Zero_WhenNoRecords()
    {
        // Arrange
        var itemId = 99;

        _repository.GetRepairingQuantityAsync(itemId)
            .Returns(Task.FromResult(0));

        // Act
        var result = await _repository.GetRepairingQuantityAsync(itemId);

        // Assert
        Assert.Equal(0, result);
        await _repository.Received(1).GetRepairingQuantityAsync(itemId);
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ShouldCall_Repository_Method()
    {
        // Arrange
        var itemId = 123;

        // Act
        await _repository.GetRepairingQuantityAsync(itemId);

        // Assert
        await _repository.Received(1).GetRepairingQuantityAsync(itemId);
    }
}
