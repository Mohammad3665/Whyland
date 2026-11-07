using Whyland.Shared.Dtos.Catalog.CourseSection;

namespace Whyland.Domain.Model.Catalog.CourseSection.Repositories;

/// <summary>Query-side repository for the sections of one course.</summary>
public interface ICourseSectionQueryRepository
{
    /// <summary>Returns every live section of a course with its episode count, ordered by DisplayOrder.</summary>
    Task<IReadOnlyList<CourseSectionListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default);
}
