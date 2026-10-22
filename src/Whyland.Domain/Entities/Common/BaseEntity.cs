using Whyland.Domain.Common.Primitives;

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
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// The UTC time at which the entity was created.
    /// Populated by the audit interceptor just before save; never by clients.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The identifier of the user responsible for creating the record.
    /// Populated by the audit interceptor just before save; never by clients.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// The UTC time of the latest modification of the entity.
    /// Refreshed by the audit interceptor on every update; never by clients.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The identifier of the user responsible for the latest change.
    /// Refreshed by the audit interceptor on every update; never by clients.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

}
