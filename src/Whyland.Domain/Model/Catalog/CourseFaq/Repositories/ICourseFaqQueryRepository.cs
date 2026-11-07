using Whyland.Shared.Dtos.Catalog.CourseFaq;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Repositories;

/// <summary>Query-side repository for the FAQs attached to one course.</summary>
public interface ICourseFaqQueryRepository
{
    /// <summary>Returns every live FAQ of a course ordered by DisplayOrder.</summary>
    Task<IReadOnlyList<CourseFaqListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default);
}
