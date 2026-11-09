using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.CourseSection;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseSection;

/// <summary>
/// Query-side repository for the <see cref="CourseSection"/> aggregate.
/// Implemented with the shared Dapper query builder, so the table, the columns, the filters and the
/// ordering are declared once from typed properties instead of a hand-written statement.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class CourseSectionQueryRepository(NpgsqlDataSource dataSource) : ICourseSectionQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;

    /// <summary>The quoted identifier column the episode subquery correlates on.</summary>
    private static readonly string SectionIdColumn =
        DapperFluentQueryBuilder<CourseSectionEntity>.RootColumn(nameof(CourseSectionEntity.Id));

    /// <summary>
    /// Counts the live episodes of a section on every read, as s7.6 requires; no denormalized
    /// counter has to be kept in sync when episodes arrive.
    /// </summary>
    private static readonly string EpisodeCountColumn = $"""
        (SELECT COUNT(*)::int
         FROM {catalog}."Episodes" e
         WHERE e."SectionId" = {SectionIdColumn}
           AND e."IsDeleted" = FALSE)
        """;

    /// <summary>
    /// Returns the live sections of one course with their episode count, ordered by DisplayOrder
    /// and then by Id so two rows that share a position still come back in a stable order.
    /// </summary>
    /// <param name="courseId">The course whose sections are read.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<IReadOnlyList<CourseSectionListItemDto>> GetForCourseAsync(
        CourseId courseId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<CourseSectionEntity>.Create(
                $"{catalog}.CourseSections",
                x => x.Id,
                x => x.CourseId,
                x => x.Title,
                x => x.DisplayOrder,
                x => x.CreatedAt)
            .SelectExpression(EpisodeCountColumn, nameof(CourseSectionListItemDto.EpisodeCount))
            .Where(x => x.CourseId, courseId.Value)
            .OrderBy(x => x.DisplayOrder)
            .TieBreakBy(x => x.Id)
            .GetAllAsync<CourseSectionListItemDto>(dataSource, ct);
}
