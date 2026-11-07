using Whyland.Shared.Dtos.Catalog.CourseFaq;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseFaq;

/// <summary>Dapper read side of the FAQs attached to one course.</summary>
public sealed class CourseFaqQueryRepository(NpgsqlDataSource dataSource) : ICourseFaqQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;

    private const string Sql = $"""
        SELECT f."Id", f."CourseId", f."Title", f."Description", f."DisplayOrder", f."CreatedAt"
        FROM {catalog}."CourseFaqs" f
        WHERE f."CourseId" = @courseId AND f."IsDeleted" = FALSE
        ORDER BY f."DisplayOrder" ASC, f."Id" ASC;
        """;

    public async Task<IReadOnlyList<CourseFaqListItemDto>> GetForCourseAsync(
        long courseId,
        CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<CourseFaqListItemDto>(
            new CommandDefinition(Sql, new { courseId }, cancellationToken: ct));

        return rows.AsList();
    }
}
