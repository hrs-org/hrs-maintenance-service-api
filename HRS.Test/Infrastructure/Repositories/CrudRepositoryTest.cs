using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Infrastructure;
using HRS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRS.Test.Infrastructure.Repositories;

public class CrudRepositoryTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAdd_Maintenance_Record()
    {
        var dbName = $"CrudRepo_AddAsync_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        var m = new ItemMaintenance
        {
            Id = 1,
            ItemId = 1001,
            Type = ItemMaintenanceType.Repair,
            Quantity = 3,
            Remarks = "broken lens"
        };

        await repo.AddAsync(m);
        await repo.SaveChangesAsync();

        var result = await dbContext.ItemMaintenances.FindAsync(1);
        Assert.NotNull(result);
        Assert.Equal(ItemMaintenanceType.Repair, result!.Type);
        Assert.Equal(3, result.Quantity);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturn_Maintenance_Record()
    {
        var dbName = $"CrudRepo_GetById_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        dbContext.ItemMaintenances.Add(new ItemMaintenance
        {
            Id = 2,
            ItemId = 1002,
            Type = ItemMaintenanceType.Repair,
            Quantity = 2,
            Remarks = "screen crack"
        });
        await dbContext.SaveChangesAsync();

        var result = await repo.GetByIdAsync(2);
        Assert.NotNull(result);
        Assert.Equal(1002, result!.ItemId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturn_All_Maintenance_Records()
    {
        var dbName = $"CrudRepo_GetAll_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        dbContext.ItemMaintenances.AddRange(
            new ItemMaintenance { Id = 3, ItemId = 2001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new ItemMaintenance { Id = 4, ItemId = 2002, Type = ItemMaintenanceType.Fixed, Quantity = 5 }
        );
        await dbContext.SaveChangesAsync();

        var all = (await repo.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
        Assert.Contains(all, x => x.Id == 3);
        Assert.Contains(all, x => x.Id == 4);
    }

    [Fact]
    public async Task FindAsync_ShouldFilter_By_Type()
    {
        var dbName = $"CrudRepo_Find_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        dbContext.ItemMaintenances.AddRange(
            new ItemMaintenance { Id = 5, ItemId = 3001, Type = ItemMaintenanceType.Repair, Quantity = 2 },
            new ItemMaintenance { Id = 6, ItemId = 3002, Type = ItemMaintenanceType.Fixed, Quantity = 2 }
        );
        await dbContext.SaveChangesAsync();

        var repairing = (await repo.FindAsync(x => x.Type == ItemMaintenanceType.Repair)).ToList();
        Assert.Single(repairing);
        Assert.Equal(5, repairing[0].Id);
    }

    [Fact]
    public async Task Update_ShouldModify_Maintenance_Record()
    {
        var dbName = $"CrudRepo_Update_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        var m = new ItemMaintenance { Id = 7, ItemId = 4001, Type = ItemMaintenanceType.Repair, Quantity = 1, Remarks = "old" };
        dbContext.ItemMaintenances.Add(m);
        await dbContext.SaveChangesAsync();

        m.Quantity = 4;
        m.Remarks  = "updated";
        repo.Update(m);
        await repo.SaveChangesAsync();

        var reloaded = await dbContext.ItemMaintenances.FindAsync(7);
        Assert.Equal(4, reloaded!.Quantity);
        Assert.Equal("updated", reloaded.Remarks);
    }

    [Fact]
    public async Task Remove_ShouldDelete_Maintenance_Record()
    {
        var dbName = $"CrudRepo_Remove_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        var m = new ItemMaintenance { Id = 8, ItemId = 5001, Type = ItemMaintenanceType.Repair, Quantity = 1 };
        dbContext.ItemMaintenances.Add(m);
        await dbContext.SaveChangesAsync();

        repo.Remove(m);
        await repo.SaveChangesAsync();

        var found = await dbContext.ItemMaintenances.FindAsync(8);
        Assert.Null(found);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAdd_Multiple_Maintenance_Records()
    {
        var dbName = $"CrudRepo_AddRange_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        var list = new[]
        {
            new ItemMaintenance { Id = 9,  ItemId = 6001, Type = ItemMaintenanceType.Repair, Quantity = 2 },
            new ItemMaintenance { Id = 10, ItemId = 6002, Type = ItemMaintenanceType.Repair, Quantity = 3 }
        };

        await repo.AddRangeAsync(list);
        await repo.SaveChangesAsync();

        var all = await dbContext.ItemMaintenances.ToListAsync();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task RemoveRange_ShouldDelete_Multiple_Maintenance_Records()
    {
        var dbName = $"CrudRepo_RemoveRange_{Guid.NewGuid()}";
        using var dbContext = CreateDbContext(dbName);
        var repo = new CrudRepository<ItemMaintenance>(dbContext);

        var list = new[]
        {
            new ItemMaintenance { Id = 11, ItemId = 7001, Type = ItemMaintenanceType.Repair, Quantity = 1 },
            new ItemMaintenance { Id = 12, ItemId = 7002, Type = ItemMaintenanceType.Repair, Quantity = 1 }
        };
        dbContext.ItemMaintenances.AddRange(list);
        await dbContext.SaveChangesAsync();

        repo.RemoveRange(list);
        await repo.SaveChangesAsync();

        var all = await dbContext.ItemMaintenances.ToListAsync();
        Assert.Empty(all);
    }
}
