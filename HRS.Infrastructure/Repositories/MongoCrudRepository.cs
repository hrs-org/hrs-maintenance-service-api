using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using HRS.Domain.Interfaces;

namespace HRS.Infrastructure.Repositories;

public class MongoCrudRepository<T> : ICrudRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoCrudRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        var filter = Builders<T>.Filter.Eq("_id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<System.Func<T, bool>> predicate)
    {
        var all = await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
        return all.Where(predicate.Compile());
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public void Update(T entity)
    {
        var idProp = typeof(T).GetProperty("_id");
        var id = idProp?.GetValue(entity);
        if (id == null) throw new InvalidOperationException("Entity must have _id property for MongoDB update.");
        var filter = Builders<T>.Filter.Eq("_id", id);
        _collection.ReplaceOne(filter, entity);
    }

    public void Remove(T entity)
    {
        var idProp = typeof(T).GetProperty("_id");
        var id = idProp?.GetValue(entity);
        if (id == null) throw new InvalidOperationException("Entity must have _id property for MongoDB delete.");
        var filter = Builders<T>.Filter.Eq("_id", id);
        _collection.DeleteOne(filter);
    }

    public void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Remove(entity);
        }
    }

    public Task<int> SaveChangesAsync()
    {
        // MongoDB 操作为即时生效，无需 SaveChanges
        return Task.FromResult(0);
    }

    public Task<object> BeginTransactionAsync()
    {
        // MongoDB 事务需特殊处理，暂不实现
        return Task.FromResult<object>(null!);
    }
}
