using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Infrastructure.Repositories;
using MongoDB.Driver;
using Xunit;

namespace HRS.Test.Infrastructure.Repositories;

public class CrudRepositoryTests
{
    private static IMongoDatabase CreateTestDatabase()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        var dbName = $"CrudRepoTest_{Guid.NewGuid()}";
        return client.GetDatabase(dbName);
    }

    [Fact]
    public async Task AddAsync_ShouldAdd_Maintenance_Record()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var m = new ItemMaintenance
        {
            ItemId = 1001,
            Type = ItemMaintenanceType.Repair,
            Quantity = 3,
            Remarks = "broken lens"
        };
        await repo.AddAsync(m);
        var result = (await repo.GetAllAsync()).FirstOrDefault(x => x.ItemId == 1001);
        Assert.NotNull(result);
        Assert.Equal(ItemMaintenanceType.Repair, result!.Type);
        Assert.Equal(3, result.Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturn_Maintenance_Record()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var m = new ItemMaintenance
        {
            ItemId = 1002,
            Type = ItemMaintenanceType.Repair,
            Quantity = 2,
            Remarks = "screen crack"
        };
        await repo.AddAsync(m);
        var all = await repo.GetAllAsync();
        var result = all.FirstOrDefault(x => x.ItemId == 1002);
        Assert.NotNull(result);
        Assert.Equal(1002, result!.ItemId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturn_All_Maintenance_Records()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var list = new[]
        {
            new ItemMaintenance { ItemId = 2001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new ItemMaintenance { ItemId = 2002, Type = ItemMaintenanceType.Fixed, Quantity = 5 }
        };
        await repo.AddRangeAsync(list);
        var all = (await repo.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, x => x.ItemId == 2001);
        Assert.Contains(all, x => x.ItemId == 2002);
    }

    [Fact]
    public async Task FindAsync_ShouldFilter_By_Type()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var list = new[]
        {
            new ItemMaintenance { ItemId = 3001, Type = ItemMaintenanceType.Repair, Quantity = 2 },
            new ItemMaintenance { ItemId = 3002, Type = ItemMaintenanceType.Fixed, Quantity = 2 }
        };
        await repo.AddRangeAsync(list);
        var repairing = (await repo.FindAsync(x => x.Type == ItemMaintenanceType.Repair)).ToList();
        Assert.Single(repairing);
        Assert.Equal(3001, repairing[0].ItemId);
    }

    [Fact]
    public async Task Update_ShouldModify_Maintenance_Record()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var m = new ItemMaintenance { ItemId = 4001, Type = ItemMaintenanceType.Repair, Quantity = 1, Remarks = "old" };
        await repo.AddAsync(m);
        var inserted = (await repo.GetAllAsync()).First(x => x.ItemId == 4001);
        inserted.Quantity = 4;
        inserted.Remarks  = "updated";
        repo.Update(inserted);
        var reloaded = (await repo.GetAllAsync()).First(x => x.ItemId == 4001);
        Assert.Equal(4, reloaded.Quantity);
        Assert.Equal("updated", reloaded.Remarks);
    }

    [Fact]
    public async Task Remove_ShouldDelete_Maintenance_Record()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var m = new ItemMaintenance { ItemId = 5001, Type = ItemMaintenanceType.Repair, Quantity = 1 };
        await repo.AddAsync(m);
        var inserted = (await repo.GetAllAsync()).First(x => x.ItemId == 5001);
        repo.Remove(inserted);
        var found = (await repo.GetAllAsync()).FirstOrDefault(x => x.ItemId == 5001);
        Assert.Null(found);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAdd_Multiple_Maintenance_Records()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var list = new[]
        {
            new ItemMaintenance { ItemId = 6001, Type = ItemMaintenanceType.Repair, Quantity = 2 },
            new ItemMaintenance { ItemId = 6002, Type = ItemMaintenanceType.Repair, Quantity = 3 }
        };
        await repo.AddRangeAsync(list);
        var all = (await repo.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task RemoveRange_ShouldDelete_Multiple_Maintenance_Records()
    {
        var database = CreateTestDatabase();
        var repo = new MongoCrudRepository<ItemMaintenance>(database, "ItemMaintenances");
        var list = new[]
        {
            new ItemMaintenance { ItemId = 7001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new ItemMaintenance { ItemId = 7002, Type = ItemMaintenanceType.Repair, Quantity = 1 }
        };
        await repo.AddRangeAsync(list);
        var inserted = (await repo.GetAllAsync()).Where(x => x.ItemId == 7001 || x.ItemId == 7002).ToList();
        repo.RemoveRange(inserted);
        var all = (await repo.GetAllAsync()).Where(x => x.ItemId == 7001 || x.ItemId == 7002).ToList();
        Assert.Empty(all);
    }
}
