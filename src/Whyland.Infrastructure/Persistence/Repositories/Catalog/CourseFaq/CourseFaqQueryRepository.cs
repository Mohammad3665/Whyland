using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.CourseFaq;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseFaq;

/// <summary>
/// Query-side repository for the <see cref="CourseFaq"/> aggregate.
/// Implemented with the shared Dapper query builder, so the table, the columns, the filters and the
/// ordering are declared once from typed properties instead of a hand-written statement.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class CourseFaqQueryRepository(NpgsqlDataSource dataSource) : ICourseFaqQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;

    /// <summary>
    /// Returns the live FAQs of one course ordered by DisplayOrder, then by Id so two rows that
    /// share a position still come back in a stable order.
    /// </summary>
    /// <param name="courseId">The course whose FAQ items are read.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<IReadOnlyList<CourseFaqListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<CourseFaqEntity>.Create(
                $"{catalog}.CourseFaqs",
                x => x.Id,
                x => x.CourseId,
                x => x.Title,
                x => x.Description,
                x => x.DisplayOrder,
                x => x.CreatedAt)
            .Where(x => x.CourseId, courseId)
            .OrderBy(x => x.DisplayOrder)
            .TieBreakBy(x => x.Id)
            .GetAllAsync<CourseFaqListItemDto>(dataSource, ct);
}
