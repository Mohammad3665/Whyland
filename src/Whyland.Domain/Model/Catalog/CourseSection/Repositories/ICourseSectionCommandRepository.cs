using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;

namespace Whyland.Domain.Model.Catalog.CourseSection.Repositories;

/// <summary>
/// Command-side repository for the <c>CourseSection</c> aggregate. Implemented with EF Core
/// (change tracking); persistence goes through <c>IUnitOfWork.SaveAsync()</c>.
/// </summary>
public interface ICourseSectionCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    Task<CourseSectionEntity?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    Task AddAsync(CourseSectionEntity section, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    void Update(CourseSectionEntity section);

    /// <summary>Marks the entity as soft-deleted.</summary>
    void SoftDelete(CourseSectionEntity section);

    /// <summary>Checks whether the section still has a live episode.</summary>
    Task<bool> HasEpisodesAsync(long sectionId, CancellationToken ct = default);

    /// <summary>Stages the new display-order sequence for one course.</summary>
    Task ReorderAsync(long courseId, IReadOnlyList<long> orderedIds, CancellationToken ct = default);

    /// <summary>Stages a compaction of DisplayOrder to 1..n for one course.</summary>
    Task RenumberAsync(long courseId, CancellationToken ct = default);
}
