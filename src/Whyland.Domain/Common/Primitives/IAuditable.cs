namespace Whyland.Domain.Common.Primitives;

/// <summary>
/// Audit contract for entities that must record who created or changed them and when (s4.2).
/// Values are populated by the application layer, never by clients.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The UTC time at which the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The identifier of the user responsible for creating the record.
    /// Null for system-generated records.
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// The UTC time of the latest modification of the entity.
    /// </summary>
    DateTime UpdatedAt { get; set; }

    /// <summary>
    /// The identifier of the user responsible for the latest change.
    /// </summary>
    Guid? UpdatedBy { get; set; }
}
