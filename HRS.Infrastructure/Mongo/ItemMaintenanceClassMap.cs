using HRS.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

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
                cm.MapIdMember(c => c.Id).SetSerializer(new ObjectIdSerializer());
            });
        }
    }
}
