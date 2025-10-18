using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Infrastructure.Repositories;
using MongoDB.Driver;
using Xunit;

namespace HRS.Test.Infrastructure.Repositories;

public class ItemMaintenanceRepositoryTests
{
    private static IMongoDatabase CreateTestDatabase()
    {
        var client = new MongoClient("mongodb://localhost:27017"); // 可根据实际测试环境调整
        var dbName = $"ItemMaintenanceRepoTest_{Guid.NewGuid()}";
        return client.GetDatabase(dbName);
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ReturnsCorrectSum()
    {
        // Arrange
        var database = CreateTestDatabase();
        var repo = new MongoItemMaintenanceRepository(database);
        var itemId = 1;
        var maintenances = new[]
        {
            new ItemMaintenance { ItemId = itemId, Type = ItemMaintenanceType.Repair, Quantity = 5, QuantityFixed = 2 }, // 5-2=3
            new ItemMaintenance { ItemId = itemId, Type = ItemMaintenanceType.Repair, Quantity = 4, QuantityFixed = null }, // 4-0=4
            new ItemMaintenance { ItemId = itemId, Type = ItemMaintenanceType.Broken, Quantity = 10, QuantityFixed = null }, // not counted
            new ItemMaintenance { ItemId = 2, Type = ItemMaintenanceType.Repair, Quantity = 7, QuantityFixed = 1 } // not counted
        };
        await database.GetCollection<ItemMaintenance>("ItemMaintenances").InsertManyAsync(maintenances);

        // Act
        var repairing = await repo.GetRepairingQuantityAsync(itemId);

        // Assert
        Assert.Equal(7, repairing);
    }

    [Fact]
    public async Task GetRepairingQuantityAsync_ReturnsZeroIfNoneFound()
    {
        // Arrange
        var database = CreateTestDatabase();
        var repo = new MongoItemMaintenanceRepository(database);

        // Act
        var repairing = await repo.GetRepairingQuantityAsync(99);

        // Assert
        Assert.Equal(0, repairing);
    }
}
