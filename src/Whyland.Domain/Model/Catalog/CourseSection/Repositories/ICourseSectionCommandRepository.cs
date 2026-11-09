using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;

namespace Whyland.Domain.Model.Catalog.CourseSection.Repositories;

/// <summary>
/// Command-side repository for the <see cref="CourseSectionEntity"/> aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface ICourseSectionCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    /// <param name="id">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked section, or null if not found.</returns>
    Task<CourseSectionEntity?> GetByIdAsync(CourseSectionId id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    /// <param name="section">The section to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(CourseSectionEntity section, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    /// <param name="section">The tracked section to update.</param>
    void Update(CourseSectionEntity section);

    /// <summary>Marks the entity as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="section">The tracked section to soft-delete.</param>
    void SoftDelete(CourseSectionEntity section);

    /// <summary>Checks whether the section still has a live episode.</summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when at least one live episode belongs to the section.</returns>
    Task<bool> HasEpisodesAsync(CourseSectionId sectionId, CancellationToken ct = default);

    /// <summary>Stages the new display-order sequence for one course.</summary>
    /// <param name="courseId">The course whose curriculum is reordered.</param>
    /// <param name="orderedIds">The section ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(CourseId courseId, IReadOnlyList<CourseSectionId> orderedIds, CancellationToken ct = default);

    /// <summary>Stages a compaction of DisplayOrder to 1..n for one course.</summary>
    /// <param name="courseId">The course whose curriculum is renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CourseId courseId, CancellationToken ct = default);
}
