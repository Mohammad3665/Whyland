using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.Course.Repositories;

/// <summary>
/// Command-side repository for the <see cref="CourseEntity"/> aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface ICourseCommandRepository
{
    #region Reads

    /// <summary>
    /// Returns the tracked course together with its category and instructor links, or null when
    /// there is none with that identifier.
    /// </summary>
    /// <param name="id">Course identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked course, or null if not found.</returns>
    Task<CourseEntity?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Whether another live course already uses this slug. A deleted course frees its slug for a
    /// new row, matching the filtered unique index.
    /// </summary>
    /// <param name="slug">The slug to look for.</param>
    /// <param name="excludeId">A row that must not count against itself, when editing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when another live course already uses the slug.</returns>
    Task<bool> IsSlugTakenAsync(string slug, long? excludeId = null, CancellationToken ct = default);

    #endregion

    #region Writes

    /// <summary>Stages a new course; persisted on SaveAsync.</summary>
    /// <param name="course">The course to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(CourseEntity course, CancellationToken ct = default);

    /// <summary>Stages an existing course for update; persisted on SaveAsync.</summary>
    /// <param name="course">The tracked course to update.</param>
    void Update(CourseEntity course);

    /// <summary>Marks the course as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="course">The tracked course to soft-delete.</param>
    void SoftDelete(CourseEntity course);

    #endregion
}
