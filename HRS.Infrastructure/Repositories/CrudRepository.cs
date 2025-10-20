using System.Linq.Expressions;
using HRS.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HRS.Infrastructure.Repositories;

public class CrudRepository<T> : ICrudRepository<T> where T : class
{
    protected readonly IMongoDatabase _db;
    protected readonly IMongoCollection<T> _collection;

    public CrudRepository(IMongoDatabase db, string collectionName)
    {
        _db = db;
        _collection = db.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(ObjectId id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        if (ObjectId.TryParse(id, out var oid))
        {
            return await GetByIdAsync(oid);
        }

        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public Task<T?> GetByIdAsync(object id) => throw new NotImplementedException();

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _collection.InsertManyAsync(entities);
    }

    public virtual void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Try to get the Id property using reflection
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} must have an 'Id' property");

        var idValue = idProperty.GetValue(entity);
        if (idValue == null)
            throw new InvalidOperationException("Entity Id cannot be null");

        var filter = Builders<T>.Filter.Eq("Id", idValue);
        _collection.ReplaceOne(filter, entity);
    }

    public virtual void Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        // Try to get the Id property using reflection
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} must have an 'Id' property");

        var idValue = idProperty.GetValue(entity);
        if (idValue == null)
            throw new InvalidOperationException("Entity Id cannot be null");

        var filter = Builders<T>.Filter.Eq("Id", idValue);
        _collection.DeleteOne(filter);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities)
        {
            Remove(entity);
        }
    }

    public virtual Task<int> SaveChangesAsync()
    {
        return Task.FromResult(0);
    }

    public virtual Task<object> BeginTransactionAsync()
    {
        throw new NotSupportedException("Transactions are not supported by this repository.");
    }
}
