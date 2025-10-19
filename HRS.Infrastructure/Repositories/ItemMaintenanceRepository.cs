using HRS.Domain.Entities;
using HRS.Domain.Enums;
using HRS.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Bson;

namespace HRS.Infrastructure.Repositories;
public class ItemMaintenanceRepository : CrudRepository<ItemMaintenance>, IItemMaintenanceRepository
{
    public ItemMaintenanceRepository(IMongoDatabase database) : base(database, "ItemMaintenances")
    {
    }

    public override void Update(ItemMaintenance entity)
    {
        var filter = Builders<ItemMaintenance>.Filter.Eq("Id", entity.Id);
        _collection.ReplaceOne(filter, entity);
    }

    public override void Remove(ItemMaintenance entity)
    {
        var filter = Builders<ItemMaintenance>.Filter.Eq("Id", entity.Id);
        _collection.DeleteOne(filter);
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
