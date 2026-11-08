using Whyland.Shared.Dtos.Catalog.CourseSection;

namespace Whyland.Domain.Model.Catalog.CourseSection.Repositories;

/// <summary>
/// Query-side repository for the <see cref="CourseSection"/> aggregate.
/// Implemented with Dapper for lightweight, non-tracked read queries.
/// </summary>
public interface ICourseSectionQueryRepository
{
    /// <summary>
    /// Returns every live section of a course with its episode count, ordered by DisplayOrder.
    /// </summary>
    /// <param name="courseId">The course whose sections are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of sections ordered by DisplayOrder.</returns>
    Task<IReadOnlyList<CourseSectionListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default);
}
