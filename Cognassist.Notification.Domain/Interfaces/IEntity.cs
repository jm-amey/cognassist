namespace Cognassist.Notification.Domain;

/// <summary>
/// A default <see cref="IEntity{TKey}"/> for the default primary key type of <see cref="string"/>
/// </summary>
public interface IEntity : IEntity<string>
{
}

/// <summary>
/// Defines interface for base entity type. All entities in the system must implement this interface.
/// </summary>
/// <typeparam name="TKey">Type of the primary key of the entity. Default is <see cref="string"/></typeparam>
public interface IEntity<TKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    TKey Id { get; set; }
}