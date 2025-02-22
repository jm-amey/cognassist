using System.Linq.Expressions;
using Cognassist.Notification.Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using static System.Net.HttpStatusCode;

namespace Cognassist.Notification.ComsosDb;

public class CosmosDbRepository<TEntity>(Container container)
       : IRepository<TEntity> where TEntity : class, IEntity
{
    protected readonly string entityName = typeof(TEntity).Name;

    protected readonly CosmosLinqSerializerOptions linqSerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase };

    /// <inheritdoc/>
    public virtual async ValueTask<TEntity> CreateAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = GetPartitionKey(entity.Id, partitionKeyValue);

        try
        {
            var response = await container.CreateItemAsync(entity, partitionKey, cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == Conflict)
        {
            throw new ConflictException($"{entityName} with same Id: {entity.Id} exists", ex);
        }
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TEntity> GetByIdAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = GetPartitionKey(id, partitionKeyValue);

        try
        {
            var response = await container.ReadItemAsync<TEntity>(id, partitionKey, cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public virtual IQueryable<T> GetQueryableAsync<T>(Expression<Func<T, bool>>? predicate = null)
    where T : TEntity
    {
        var queryable = container.GetItemLinqQueryable<T>(linqSerializerOptions: linqSerializerOptions).AsQueryable();
        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        return queryable;
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var queryable = container.GetItemLinqQueryable<TEntity>(linqSerializerOptions: linqSerializerOptions).AsQueryable();
        if (predicate is not null)
        {
            queryable = queryable.Where(predicate);
        }

        var iterator = queryable.ToFeedIterator();

        await foreach (var result in IterateAsync(iterator, cancellationToken))
        {
            yield return result;
        }
    }

    /// <inheritdoc/>
    public virtual async IAsyncEnumerable<TEntity> GetAsync(string query, IReadOnlyDictionary<string, object>? parameters = default, CancellationToken cancellationToken = default)
    {
        var queryDefinition = new QueryDefinition(query);
        if (parameters is not null)
        {
            foreach (var parameter in parameters)
            {
                queryDefinition.WithParameter(parameter.Key, parameter.Value);
            }
        }

        var iterator = container.GetItemQueryIterator<TEntity>(queryDefinition);

        await foreach (var result in IterateAsync(iterator, cancellationToken))
        {
            yield return result;
        }
    }

    public virtual async ValueTask<TEntity> UpdateAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var partitionKey = GetPartitionKey(entity.Id, partitionKeyValue);
            var response = await container.ReplaceItemAsync(entity, entity.Id, partitionKey, cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            throw new NotFoundException($"{entityName} with id: {entity.Id} not found.", ex);
        }
    }

    public virtual async ValueTask<TEntity> PatchAsync(string id, string? partitionKeyValue, (string path, object value)[] operations, CancellationToken cancellationToken = default)
    {
        try
        {
            var partitionKey = GetPartitionKey(id, partitionKeyValue);
            var patchSetOperations = operations
                .Select(operation => PatchOperation.Set(operation.path, operation.value))
                .ToList();

            var response = await container.PatchItemAsync<TEntity>(id, partitionKey,
                patchSetOperations, new PatchItemRequestOptions()
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                },
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            throw new NotFoundException($"{entityName} with id: {id} not found", ex);
        }
    }

    public virtual async ValueTask<TEntity> PatchAsync(string id, string partitionKeyValue, (string path, object value, PatchOperationType operationType)[] operations, CancellationToken cancellationToken = default)
    {
        try
        {
            var partitionKey = GetPartitionKey(id, partitionKeyValue);

            var patchCosmosOperations = operations.Select(CosmosDbRepository<TEntity>.MapPatchOperation);

            var response = await container.PatchItemAsync<TEntity>(id, partitionKey,
                [.. patchCosmosOperations], new PatchItemRequestOptions()
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                },
                cancellationToken: cancellationToken);

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            throw new NotFoundException($"{entityName} with id: {id} not found", ex);
        }
    }

    public virtual async ValueTask<TEntity> UpsertAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = GetPartitionKey(entity.Id, partitionKeyValue);
        var response = await container.UpsertItemAsync(entity, partitionKey, cancellationToken: cancellationToken);

        return response.Resource;
    }

    public virtual async ValueTask DeleteAsync(string id, string? partitionKeyValue = null, CancellationToken cancellationToken = default)
    {
        var partitionKey = GetPartitionKey(id, partitionKeyValue);
        try
        {
            await container.DeleteItemAsync<TEntity>(id, partitionKey, cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            throw new NotFoundException($"{entityName} with id: {id} not found", ex);
        }
    }

    public virtual async ValueTask DropPartition(string partitionKeyValue, CancellationToken cancellationToken = default)
    {
        var partitionKey = GetPartitionKey(string.Empty, partitionKeyValue);
        try
        {
            await container.DeleteAllItemsByPartitionKeyStreamAsync(partitionKey, cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == NotFound)
        {
            throw new NotFoundException($"{entityName} with id: {partitionKey} not found", ex);
        }
    }

    public static async IAsyncEnumerable<TResult> IterateAsync<TResult>(FeedIterator<TResult> iterator, CancellationToken cancellationToken = default)
    {
        var resultSet = new List<TResult>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);

            foreach (var item in response)
            {
                yield return item;
            }
        }
    }


    private static PatchOperation MapPatchOperation((string path, object value, PatchOperationType operationType) operation)
    {
        return operation.operationType switch
        {
            PatchOperationType.Add => PatchOperation.Add(operation.path, operation.value),
            PatchOperationType.Set => PatchOperation.Set(operation.path, operation.value),
            PatchOperationType.Replace => PatchOperation.Replace(operation.path, operation.value),
            _ => throw new ArgumentException("Invalid patch operation enum value"),
        };
    }

    protected static PartitionKey GetPartitionKey(string id, string? partitionKeyValue)
    {
        return string.IsNullOrWhiteSpace(partitionKeyValue) ? new PartitionKey(id) : new PartitionKey(partitionKeyValue);
    }
}