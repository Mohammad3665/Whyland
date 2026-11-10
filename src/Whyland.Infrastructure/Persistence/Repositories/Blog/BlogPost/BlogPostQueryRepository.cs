using Whyland.Domain.Model.Blog.BlogPost.Repositories;
using Whyland.Infrastructure.Persistence.Extensions;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Blog.BlogPost;
using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Infrastructure.Persistence.Repositories.Blog.BlogPost;

/// <summary>
/// The Dapper query repository for the <c>BlogPost</c> aggregate.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public class BlogPostQueryRepository(NpgsqlDataSource dataSource) : IBlogPostQueryRepository
{
    private const string blog = SchemaConstants.Blog;

    /// <summary>The joined author is read under this alias, never through the audit join names.</summary>
    private const string AuthorAlias = "a";

    /// <summary>
    /// Public image URLs are derived in SQL from the one stored relative path, exactly like a course
    /// image. Storing a URL would make the database own the web root, and moving the thumbnail
    /// directory would need a data migration.
    /// </summary>
    private const string ImageUrlColumn = """
        CASE WHEN p."Image" IS NULL OR p."Image" = ''
             THEN NULL
             ELSE '/uploads/images/' || p."Image" END
        """;

    private const string ThumbnailUrlColumn = """
        CASE WHEN p."Image" IS NULL OR p."Image" = ''
             THEN NULL
             ELSE replace('/uploads/images/' || p."Image", '/originals/', '/thumbnails/') END
        """;

    /// <summary>
    /// The visibility enum is stored as its numeric value by EF's convention, while the row DTO
    /// carries the name so the table can dress it without knowing the numbers. The CASE also keeps
    /// the generic picker working: it sends the name and compares it to this same text expression.
    /// </summary>
    private const string DisplayStatusColumn = """
        CASE p."DisplayStatus"
            WHEN 1 THEN 'Published'
            WHEN 2 THEN 'Hidden'
            ELSE 'Draft' END
        """;

    private const string AuthorNameColumn = """
        a."FirstName" || ' ' || a."LastName"
        """;

    private const string CategoryNameColumn = """
        bc."Name"
        """;

    private const string DeletedByNameColumn = """
        du."FirstName" || ' ' || du."LastName"
        """;

    /// <summary>
    /// The admin list. It joins the category for its name, the author for the person's name, and the
    /// audit/deleted authors for readable names instead of guids. The row carries the article body,
    /// so the edit form and details modal are filled from the page's row cache without another
    /// request.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {blog}."BlogPosts" p
            JOIN {blog}."BlogCategories" bc ON bc."Id" = p."CategoryId" AND bc."IsDeleted" = FALSE
            LEFT JOIN {SchemaConstants.Identity}."Users" {AuthorAlias}
                ON {AuthorAlias}."Id" = p."AuthorId"
            {SqlDataTableSpec.AuthorJoin("p", SqlDataTableSpec.CreatedByAlias, "CreatedBy")}
            {SqlDataTableSpec.AuthorJoin("p", SqlDataTableSpec.UpdatedByAlias, "UpdatedBy")}
            {SqlDataTableSpec.AuthorJoin("p", "du", "DeletedBy")}
            """,
        baseWhere: """p."IsDeleted" = FALSE""",
        defaultOrder: """p."DisplayOrder" ASC, p."Id" DESC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<BlogPostEntity>("p"),
            [
                ("ImageUrl", ImageUrlColumn, typeof(string)),
                ("ThumbnailUrl", ThumbnailUrlColumn, typeof(string)),
                ("DisplayStatus", DisplayStatusColumn, typeof(string)),
                ("AuthorName", AuthorNameColumn, typeof(string)),
                ("CategoryName", CategoryNameColumn, typeof(string)),
                .. SqlDataTableSpec.AuthorNames(),
                ("DeletedByName", DeletedByNameColumn, typeof(string)),
            ]),
        searchColumns:
        [
            nameof(BlogPostEntity.Title),
            nameof(BlogPostEntity.ShortDescription),
            nameof(BlogPostEntity.Slug),
        ]);

    public Task<DataTableResult<BlogPostListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<BlogPostListItemDto>(dataSource, filter, conditions, ct);
}
