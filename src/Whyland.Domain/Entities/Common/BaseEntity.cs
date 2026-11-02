namespace Whyland.Domain.Entities.Common;

/// <summary>
/// Base type for all persisted business entities.
/// Carries the mandatory audit fields (s4.2 of the SRS).
/// Audit values must always be populated by the server/application layer, never by clients.
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key (e.g. int, Guid, string).</typeparam>
public abstract class BaseEntity<TKey> : IEntity, IAuditable
{
    /// <summary>
    /// The unique identifier of the entity. A single consistent ID strategy is used across the system.
    /// </summary>
    public TKey Id { get; protected set; } = default!;

    /// <summary>
    /// The UTC time at which the entity was created.
    /// Populated by the audit interceptor just before save; never by clients.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// The identifier of the user responsible for creating the record.
    /// Populated by the audit interceptor just before save; never by clients.
    /// </summary>
    public Guid? CreatedBy { get; private set; }

    /// <summary>
    /// The UTC time of the latest modification of the entity.
    /// Refreshed by the audit interceptor on every update; never by clients.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// The identifier of the user responsible for the latest change.
    /// Refreshed by the audit interceptor on every update; never by clients.
    /// </summary>
    public Guid? UpdatedBy { get; private set; }
}
