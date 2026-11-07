using Whyland.Shared.Dtos.Catalog.CourseSection;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseSection;

/// <summary>Dapper read side of the sections belonging to one course.</summary>
public sealed class CourseSectionQueryRepository(NpgsqlDataSource dataSource) : ICourseSectionQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;

    /// <summary>
    /// The count is derived from the episodes table on every read, as s7.6 requires; no
    /// denormalized counter has to be kept in sync when episodes arrive.
    /// </summary>
    private const string Sql = $"""
        SELECT cs."Id", cs."CourseId", cs."Title", cs."DisplayOrder", cs."CreatedAt",
               (SELECT COUNT(*)::int
                FROM {catalog}."Episodes" e
                WHERE e."SectionId" = cs."Id" AND e."IsDeleted" = FALSE) AS "EpisodeCount"
        FROM {catalog}."CourseSections" cs
        WHERE cs."CourseId" = @courseId AND cs."IsDeleted" = FALSE
        ORDER BY cs."DisplayOrder" ASC, cs."Id" ASC;
        """;

    public async Task<IReadOnlyList<CourseSectionListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<CourseSectionListItemDto>(
            new CommandDefinition(Sql, new { courseId }, cancellationToken: ct));

        return rows.AsList();
    }
}
