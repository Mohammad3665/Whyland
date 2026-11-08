using Whyland.Shared.Dtos.Catalog.CourseFaq;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Repositories;

/// <summary>
/// Query-side repository for the <see cref="CourseFaq"/> aggregate.
/// Implemented with Dapper for lightweight, non-tracked read queries.
/// </summary>
public interface ICourseFaqQueryRepository
{
    /// <summary>
    /// Returns every live FAQ of a course ordered by DisplayOrder.
    /// </summary>
    /// <param name="courseId">The course whose FAQ items are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of the course FAQ items ordered by DisplayOrder.</returns>
    Task<IReadOnlyList<CourseFaqListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default);
}
