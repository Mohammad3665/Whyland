using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Repositories;

/// <summary>
/// Command-side repository for the <see cref="CourseFaqEntity"/> aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface ICourseFaqCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    /// <param name="id">Course FAQ identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked question, or null if not found.</returns>
    Task<CourseFaqEntity?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    /// <param name="faq">The question to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(CourseFaqEntity faq, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    /// <param name="faq">The tracked question to update.</param>
    void Update(CourseFaqEntity faq);

    /// <summary>Marks the entity as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="faq">The tracked question to soft-delete.</param>
    void SoftDelete(CourseFaqEntity faq);

    /// <summary>Stages the new display-order sequence for one course.</summary>
    /// <param name="courseId">The course whose FAQ list is reordered.</param>
    /// <param name="orderedIds">The question ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(long courseId, IReadOnlyList<long> orderedIds, CancellationToken ct = default);

    /// <summary>Stages a compaction of DisplayOrder to 1..n for one course.</summary>
    /// <param name="courseId">The course whose FAQ list is renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(long courseId, CancellationToken ct = default);
}
