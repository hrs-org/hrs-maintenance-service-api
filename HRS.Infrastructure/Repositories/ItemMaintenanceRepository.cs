using HRS.Domain.Entities;
using HRS.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Bson;
using HRS.Shared.Core.Enums;

namespace HRS.Infrastructure.Repositories;
public class ItemMaintenanceRepository : CrudRepository<ItemMaintenance>, IItemMaintenanceRepository
{
    public ItemMaintenanceRepository(IMongoDatabase database) : base(database, "ItemMaintenances")
    {
    }

    public async Task<int> GetRepairingQuantityAsync(int itemId)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "ItemId", itemId },
                { "Type", (int)ItemMaintenanceType.Repair }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "quantityToSum", new BsonDocument("$subtract", new BsonArray
                    {
                        "$Quantity",
                        new BsonDocument("$ifNull", new BsonArray { "$QuantityFixed", 0 })
                    })
                }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "total", new BsonDocument("$sum", "$quantityToSum") }
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync();
        return result?["total"]?.AsInt32 ?? 0;
    }
}
