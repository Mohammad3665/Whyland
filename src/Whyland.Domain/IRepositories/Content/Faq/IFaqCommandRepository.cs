namespace Whyland.Domain.IRepositories.Content.Faq;

/// <summary>
/// Command-side repository for the Faq aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IFaqCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    /// <param name="id">Faq identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked Faq, or null if not found.</returns>
    Task<Entities.Content.Faq?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    /// <param name="faq">The Faq to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(Entities.Content.Faq faq, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    /// <param name="faq">The tracked Faq to update.</param>
    void Update(Entities.Content.Faq faq);

    /// <summary>Marks the entity as soft-deleted (hidden by global query filter).</summary>
    /// <param name="faq">The tracked Faq to soft-delete.</param>
    void SoftDelete(Entities.Content.Faq faq);

    /// <summary>Checks whether a non-deleted Faq with the given title exists.</summary>
    /// <param name="title">Title to check.</param>
    /// <param name="excludeId">Optional id to exclude (for update scenarios).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if a matching non-deleted Faq exists.</returns>
    Task<bool> ExistsAsync(string title, long? excludeId = null, CancellationToken ct = default);

    /// <summary>Stages a new display-order sequence; persisted on SaveAsync.</summary>
    /// <param name="orderedIds">The Faq ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(IReadOnlyList<long> orderedIds, CancellationToken ct = default);

    /// <summary>
    /// Stages a compaction of DisplayOrder over every live Faq so the values
    /// form an unbroken 1..n sequence (used after a delete closes a gap);
    /// persisted on SaveAsync.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CancellationToken ct = default);
}