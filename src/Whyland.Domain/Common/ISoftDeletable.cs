namespace Whyland.Domain.Common;

/// <summary>
/// Marks entities that support soft deletion (s4.1 of the SRS).
/// Soft delete is the default deletion strategy; hard delete requires an explicit justification.
/// Implementation details (global query filters) are handled in the Infrastructure layer.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    bool IsDeleted { get; set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    Guid? DeletedBy { get; set; }
}
