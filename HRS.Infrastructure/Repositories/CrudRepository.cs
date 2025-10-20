using System.Linq.Expressions;
using HRS.Domain.Interfaces;
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

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

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
        throw new NotSupportedException("Update operation must be implemented in derived classes or use UpdateAsync method");
    }

    public virtual void Remove(T entity)
    {
        throw new NotSupportedException("Remove operation must be implemented in derived classes or use RemoveAsync method");
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