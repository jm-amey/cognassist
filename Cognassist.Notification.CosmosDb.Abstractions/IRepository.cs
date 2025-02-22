using System.Linq.Expressions;
using Cognassist.Notification.Domain;

namespace Cognassist.Notification.ComsosDb;

/// <summary>
/// Defines a base repository interface for entity type of <typeparamref name="TEntity" /> and default primary
/// key of <see cref="string"/>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IRepository<TEntity>
    : IRepository<TEntity, string> where TEntity : class, IEntity
{
}

/// <summary>
/// Defines base repository interface for entity type of <typeparamref name="TEntity" /> and primary
/// key of <typeparamref name="TKey"/>
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IRepository<TEntity, in TKey> where TEntity : class, IEntity
{
    /// <summary>
    /// Creates the given <paramref name="entity"/> in the database.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="partitionKeyValue">The partition key value to use for the entity.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask{TEntity}"/> representing the created <see cref="TEntity"/>.</returns>
    ValueTask<TEntity> CreateAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the <typeparamref name="TEntity"/> implementation instance that corresponds to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The string identifier.</param>
    /// <param name="partitionKeyValue">The partition key value if different than the <paramref name="id"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask"/> representing the <see cref="TEntity"/> implementation class instance.</returns>
    ValueTask<TEntity> GetByIdAsync(TKey id, string? partitionKeyValue = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exposes an <see cref="IAsyncEnumerable{TEntity}>" /> iterator for <typeparamref name="TEntity"/> implementation classes that
    /// match the given <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The expression used for evaluating a matching item.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A collection of entity instances which meets the <paramref name="predicate"/> condition.</returns>
    IAsyncEnumerable<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exposes an <see cref="IAsyncEnumerable{TEntity}>" /> iterator for <typeparamref name="TEntity"/> implementation classes thatthat matches the given <paramref name="query"/>
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="parameters">A collection of parameters to use for the query</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A collection of entity instances returned by the given <paramref name="query"/> query.</returns>
    IAsyncEnumerable<TEntity> GetAsync(string query, IReadOnlyDictionary<string, object>? parameters = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an <see cref="IQueryable{TEntity}" /> collection of <typeparamref name="TEntity"/> that matches the given
    /// <paramref name="predicate"/> for querying the data store.
    /// NOTE: this doesn't actually make a call to cosmos DB.
    /// </summary>
    /// <param name="predicate">The expression used for evaluating a matching item.</param>
    /// <returns>An <see cref="IQueryable{TEntity}" /> collection of <typeparamref name="TEntity"/>.</returns>
    IQueryable<T> GetQueryableAsync<T>(Expression<Func<T, bool>>? predicate = null)
    where T : TEntity;

    /// <summary>
    /// Updates the entity that corresponds to the given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The item value to update.</param>
    /// <param name="partitionKeyValue">The partition key value if different than the <paramref name="entity"/> id.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask{TEntity}"/> representing the <see cref="TEntity"/> implementation class instance.</returns>
    ValueTask<TEntity> UpdateAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates or inserts the entity that corresponds to the given <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The item value to update.</param>
    /// <param name="partitionKeyValue">The partition key value if different than the <paramref name="entity"/> id.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask{TEntity}"/> representing the <see cref="TEntity"/> implementation class instance.</returns>
    ValueTask<TEntity> UpsertAsync(TEntity entity, string? partitionKeyValue = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the <typeparamref name="TEntity"/> implementation instance that corresponds to the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The string identifier.</param>
    /// <param name="partitionKeyValue">The partition key value if different than the <paramref name="id"/>.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask DeleteAsync(TKey id, string? partitionKeyValue = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops all documents for the given partition value.
    /// </summary>
    /// <param name="partitionKeyValue">The partition key value.</param>
    /// <param name="cancellationToken">The cancellation token to use when cancelling asynchronous operations.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask DropPartition(string partitionKeyValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the properties of the entity matching <paramref name="id"/> with the given <paramref name="operations"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="partitionKeyValue"></param>
    /// <param name="operations"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> PatchAsync(string id, string partitionKeyValue, (string path, object value)[] operations, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the properties of the entity matching <paramref name="id"/> with the given <paramref name="operations"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="partitionKeyValue"></param>
    /// <param name="operations"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<TEntity> PatchAsync(string id, string partitionKeyValue, (string path, object value, PatchOperationType operationType)[] operations, CancellationToken cancellationToken = default);
}

