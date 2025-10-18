using System.Threading.Tasks;
using MongoDB.Driver;
using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;

namespace HRS.Infrastructure.Repositories;

public class MongoItemMaintenanceRepository : MongoCrudRepository<ItemMaintenance>, IItemMaintenanceRepository
{
    public MongoItemMaintenanceRepository(IMongoDatabase database) : base(database, "ItemMaintenances")
    {
    }

    public async Task<int> GetRepairingQuantityAsync(int itemId)
    {
        var filter = Builders<ItemMaintenance>.Filter.And(
            Builders<ItemMaintenance>.Filter.Eq(x => x.ItemId, itemId),
            Builders<ItemMaintenance>.Filter.Eq(x => x.Type, ItemMaintenanceType.Repair)
        );
        var maintenances = await _collection.Find(filter).ToListAsync();
        return maintenances.Sum(m => m.Quantity - (m.QuantityFixed ?? 0));
    }
}
