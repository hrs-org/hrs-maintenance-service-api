using HRS.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace HRS.Infrastructure.Mongo;

public static class ItemMaintenanceClassMap
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(ItemMaintenance)))
        {
            BsonClassMap.RegisterClassMap<ItemMaintenance>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                  .SetIdGenerator(StringObjectIdGenerator.Instance)
                  .SetSerializer(new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.ObjectId));
            });
        }
    }
}