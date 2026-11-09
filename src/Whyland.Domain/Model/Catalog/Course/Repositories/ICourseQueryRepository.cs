using Whyland.Shared.Dtos.Catalog.Course;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Catalog.Course.Repositories;

/// <summary>
/// Query-side repository for the Course aggregate.
/// Implemented with Dapper for lightweight, non-tracked reads.
/// </summary>
public interface ICourseQueryRepository
{
    /// <summary>
    /// Returns one page of courses for the admin DataTable, carrying the cover image, the joined
    /// category and instructor names and the audit columns the table and details modal show.
    /// </summary>
    /// <param name="filter">Pagination, ordering and search criteria.</param>
    /// <param name="conditions">
    /// Generic field/operator/value conditions from the picker plus the two explicit filters
    /// (<c>CategoryId</c> and <c>InstructorProfileId</c>), ANDed together. Each name is resolved
    /// against the spec's whitelist, so an unknown one is dropped instead of reaching the SQL.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The paged list of course rows.</returns>
    Task<DataTableResult<CourseListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);

    /// <summary>Returns the editable course, including its selected category and instructor ids.</summary>
    /// <param name="id">Course identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The course details, or null if not found.</returns>
    Task<CourseDetailsDto?> GetByIdAsync(CourseId id, CancellationToken ct = default);

    /// <summary>Returns the active categories and the instructors the form and filters select from.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Both option lists the course form needs.</returns>
    Task<CourseFormOptionsDto> GetFormOptionsAsync(CancellationToken ct = default);

    /// <summary>Returns which of the requested ids still name active, non-deleted categories.</summary>
    /// <param name="ids">Category identifiers sent by the form.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The subset of the ids that are still selectable.</returns>
    Task<IReadOnlySet<CategoryId>> GetActiveCategoryIdsAsync(
        IReadOnlyCollection<CategoryId> ids,
        CancellationToken ct = default);

    /// <summary>Returns which of the requested ids still name instructors with active accounts.</summary>
    /// <param name="ids">Instructor profile identifiers sent by the form.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The subset of the ids that still have an active account.</returns>
    Task<IReadOnlySet<InstructorProfileId>> GetInstructorProfileIdsAsync(
        IReadOnlyCollection<InstructorProfileId> ids,
        CancellationToken ct = default);
}
