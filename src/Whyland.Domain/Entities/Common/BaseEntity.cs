
namespace Whyland.Domain.Entities.Common;

/// <summary>
/// Base type for all persisted business entities.
/// Carries the mandatory audit fields (s4.2 of the SRS).
/// Audit values must always be populated by the server/application layer, never by clients.
/// </summary>
/// <typeparam name="TKey">The type of the entity's primary key (e.g. int, Guid, string).</typeparam>
public abstract class BaseEntity<TKey> : ISoftDeletable
{
    /// <summary>
    /// The unique identifier of the entity. A single consistent ID strategy is used across the system.
    /// </summary>
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// The UTC time at which the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The identifier of the user responsible for creating the record.
    /// Null for system-generated records.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// The UTC time of the latest modification of the entity.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The identifier of the user responsible for the latest change.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }
}
