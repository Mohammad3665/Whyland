using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Blog.BlogCategory;
using BlogCategoryEntity = Whyland.Domain.Model.Blog.BlogCategory.Entities.BlogCategory;

namespace Whyland.Infrastructure.Persistence.Repositories.Blog.BlogCategory;

/// <summary>
/// The Dapper query repository for the <c>BlogCategory</c> aggregate.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public class BlogCategoryQueryRepository(NpgsqlDataSource dataSource) : IBlogCategoryQueryRepository
{
    const string blog = SchemaConstants.Blog;

    #region Computed columns

    // The blog taxonomy has no depth ceiling (s12.1.1), so the level and the tree order cannot be
    // expressions over a fixed number of ancestors the way the course catalogue computes them. A
    // recursive walk produces both once, and the shared data-table engine still reads, sorts and
    // filters through one flat SELECT over its result.

    /// <summary>
    /// One row per live category with the level it sits on and the path that sorts the tree.
    /// <para>
    /// A root is a live row with no live parent — not simply a null one — so a row whose parent was
    /// removed outside the delete guard still appears in the list instead of vanishing from it.
    /// </para>
    /// <para>
    /// <c>SortPath</c> is the display order and the key of every row from the root down, each padded
    /// to a fixed width: comparing the arrays element by element is what puts a parent immediately
    /// before its own children at any depth. <c>Trail</c> is the cycle breaker — a corrupted loop
    /// would otherwise recurse until the statement is cancelled.
    /// </para>
    /// </summary>
    private static readonly string TreeSource = $"""
        (
            WITH RECURSIVE tree AS (
                SELECT r."Id",
                       1 AS "Depth",
                       ARRAY[LPAD(r."DisplayOrder"::text, 10, '0'), LPAD(r."Id"::text, 19, '0')] AS "SortPath",
                       ARRAY[r."Id"] AS "Trail"
                FROM {blog}."BlogCategories" r
                WHERE r."IsDeleted" = FALSE
                  AND (r."ParentId" IS NULL
                       OR NOT EXISTS (SELECT 1
                                      FROM {blog}."BlogCategories" rp
                                      WHERE rp."Id" = r."ParentId" AND rp."IsDeleted" = FALSE))

                UNION ALL

                SELECT ch."Id",
                       t."Depth" + 1,
                       t."SortPath" || ARRAY[LPAD(ch."DisplayOrder"::text, 10, '0'), LPAD(ch."Id"::text, 19, '0')],
                       t."Trail" || ch."Id"
                FROM {blog}."BlogCategories" ch
                JOIN tree t ON t."Id" = ch."ParentId"
                WHERE ch."IsDeleted" = FALSE
                  AND NOT ch."Id" = ANY(t."Trail")
            )
            SELECT "Id", "Depth", "SortPath" FROM tree
        ) t
        """;

    /// <summary>How many live posts still point at the category; the delete guard reads this number.</summary>
    private static readonly string PostCountColumn = $"""
        (SELECT COUNT(*) FROM {blog}."BlogPosts" bp
         WHERE bp."CategoryId" = c."Id" AND bp."IsDeleted" = FALSE)::int
        """;

    #endregion

    /// <summary>
    /// The admin list of blog categories: one row per live category, joined to the tree walk for its
    /// level and sort path and to its parent for the name column. Paging, sorting and both record
    /// counts come from the shared data-table engine, so a new column is one whitelist entry here.
    /// Rows come out in tree order — a parent immediately followed by its children — because that is
    /// the only order a drag &amp; drop list of siblings can be read in.
    /// The audit authors join by their key, so the details modal says who made and who last changed
    /// the category instead of showing the guid the audit column stores.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {blog}."BlogCategories" c
            JOIN {TreeSource} ON t."Id" = c."Id"
            LEFT JOIN {blog}."BlogCategories" p ON p."Id" = c."ParentId" AND p."IsDeleted" = FALSE
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.CreatedByAlias, "CreatedBy")}
            {SqlDataTableSpec.AuthorJoin("c", SqlDataTableSpec.UpdatedByAlias, "UpdatedBy")}
            """,
        baseWhere: """c."IsDeleted" = FALSE""",
        defaultOrder: $"""t."SortPath" ASC, c."Id" ASC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<BlogCategoryEntity>("c"),
            [
                ("ParentName", """p."Name" """, typeof(string)),
                ("Depth", """t."Depth" """, typeof(int)),
                ("PostCount", PostCountColumn, typeof(int)),
                .. SqlDataTableSpec.AuthorNames(),
            ]),
        searchColumns:
        [
            nameof(BlogCategoryEntity.Name),
            nameof(BlogCategoryEntity.LatinName),
            nameof(BlogCategoryEntity.Slug),
        ]);

    public Task<DataTableResult<BlogCategoryListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<BlogCategoryListItemDto>(dataSource, filter, conditions, ct);
}