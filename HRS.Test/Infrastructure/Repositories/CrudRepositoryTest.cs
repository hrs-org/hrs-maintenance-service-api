using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using NSubstitute;
using Xunit;

namespace HRS.Test.Infrastructure.Repositories;

public class CrudRepositoryTests
{
    private readonly ICrudRepository<ItemMaintenance> _repository;

    public CrudRepositoryTests()
    {
        _repository = Substitute.For<ICrudRepository<ItemMaintenance>>();
    }

    [Fact]
    public async Task AddAsync_ShouldCall_Repository_AddAsync()
    {
        // Arrange
        var maintenance = new ItemMaintenance
        {
            ItemId = 1001,
            Type = ItemMaintenanceType.Repair,
            Quantity = 3,
            Remarks = "broken lens"
        };

        // Act
        await _repository.AddAsync(maintenance);

        // Assert
        await _repository.Received(1).AddAsync(maintenance);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturn_Expected_Entity()
    {
        // Arrange
        var expectedMaintenance = new ItemMaintenance
        {
            Id = "1",
            ItemId = 1002,
            Type = ItemMaintenanceType.Repair,
            Quantity = 2,
            Remarks = "screen crack"
        };

        _repository.GetByIdAsync("507f1f77bcf86cd799439011")
            .Returns(Task.FromResult<ItemMaintenance?>(expectedMaintenance));

        // Act
        var result = await _repository.GetByIdAsync("507f1f77bcf86cd799439011");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1002, result!.ItemId);
        Assert.Equal("screen crack", result.Remarks);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturn_All_Entities()
    {
        // Arrange
        var maintenances = new List<ItemMaintenance>
        {
            new() { Id = "1", ItemId = 2001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new() { Id = "2", ItemId = 2002, Type = ItemMaintenanceType.Fixed, Quantity = 5 }
        };

        _repository.GetAllAsync().Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(maintenances));

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.ItemId == 2001);
        Assert.Contains(result, x => x.ItemId == 2002);
    }

    [Fact]
    public async Task FindAsync_ShouldCall_Repository_FindAsync()
    {
        // Arrange
        var repairMaintenances = new List<ItemMaintenance>
        {
            new() { Id = "1", ItemId = 3001, Type = ItemMaintenanceType.Repair, Quantity = 2 }
        };

        _repository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<System.Func<ItemMaintenance, bool>>>())
            .Returns(Task.FromResult<IEnumerable<ItemMaintenance>>(repairMaintenances));

        // Act
        var result = (await _repository.FindAsync(x => x.Type == ItemMaintenanceType.Repair)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(3001, result[0].ItemId);
    }

    [Fact]
    public void Update_ShouldCall_Repository_Update()
    {
        // Arrange
        var maintenance = new ItemMaintenance 
        { 
            Id = "1",
            ItemId = 4001, 
            Type = ItemMaintenanceType.Repair, 
            Quantity = 4, 
            Remarks = "updated" 
        };

        // Act
        _repository.Update(maintenance);

        // Assert
        _repository.Received(1).Update(maintenance);
    }

    [Fact]
    public void Remove_ShouldCall_Repository_Remove()
    {
        // Arrange
        var maintenance = new ItemMaintenance 
        { 
            Id = "1",
            ItemId = 5001, 
            Type = ItemMaintenanceType.Repair, 
            Quantity = 1 
        };

        // Act
        _repository.Remove(maintenance);

        // Assert
        _repository.Received(1).Remove(maintenance);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldCall_Repository_AddRangeAsync()
    {
        // Arrange
        var maintenances = new[]
        {
            new ItemMaintenance { Id = "1", ItemId = 6001, Type = ItemMaintenanceType.Repair, Quantity = 2 },
            new ItemMaintenance { Id = "2", ItemId = 6002, Type = ItemMaintenanceType.Repair, Quantity = 3 }
        };

        // Act
        await _repository.AddRangeAsync(maintenances);

        // Assert
        await _repository.Received(1).AddRangeAsync(maintenances);
    }

    [Fact]
    public void RemoveRange_ShouldCall_Repository_RemoveRange()
    {
        // Arrange
        var maintenances = new[]
        {
            new ItemMaintenance { Id = "1", ItemId = 7001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new ItemMaintenance { Id = "2", ItemId = 7002, Type = ItemMaintenanceType.Repair, Quantity = 1 }
        };

        // Act
        _repository.RemoveRange(maintenances);

        // Assert
        _repository.Received(1).RemoveRange(maintenances);
    }
}
