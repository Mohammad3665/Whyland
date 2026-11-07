using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Repositories;

/// <summary>
/// Command-side repository for the <c>CourseFaq</c> aggregate. Implemented with EF Core
/// (change tracking); persistence goes through <c>IUnitOfWork.SaveAsync()</c>.
/// </summary>
public interface ICourseFaqCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    Task<CourseFaqEntity?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    Task AddAsync(CourseFaqEntity faq, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    void Update(CourseFaqEntity faq);

    /// <summary>Marks the entity as soft-deleted.</summary>
    void SoftDelete(CourseFaqEntity faq);

    /// <summary>Stages the new display-order sequence for one course.</summary>
    Task ReorderAsync(long courseId, IReadOnlyList<long> orderedIds, CancellationToken ct = default);

    /// <summary>Stages a compaction of DisplayOrder to 1..n for one course.</summary>
    Task RenumberAsync(long courseId, CancellationToken ct = default);
}
