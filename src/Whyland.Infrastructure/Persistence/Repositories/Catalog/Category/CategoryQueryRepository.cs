using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.Category;
using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Category;

/// <summary>
/// The Dapper query repository for the <c>Category</c> aggregate.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public class CategoryQueryRepository(NpgsqlDataSource dataSource) : ICategoryQueryRepository
{
    const string catalog = SchemaConstants.Catalog;

    #region Computed columns

    // The catalogue is at most three levels deep, so the level and the tree order are both plain
    // expressions over the two ancestors instead of a recursive query: the shared data-table engine
    // reads, sorts and filters through one flat SELECT, and a CTE would take the listing out of it.

    /// <summary>The level of the row: a root is 1. The ceiling check in the form reads this.</summary>
    private const string DepthColumn = """
        CASE WHEN c."ParentId" IS NULL THEN 1
             WHEN p."ParentId" IS NULL THEN 2
             ELSE 3 END
        """;

    /// <summary>Order of the root the row belongs to, so whole branches sort next to each other.</summary>
    private const string RootOrderColumn = """COALESCE(g."DisplayOrder", p."DisplayOrder", c."DisplayOrder")""";

    /// <summary>Order of the level-two row a branch hangs on, 0 for a root so it leads its children.</summary>
    private const string MiddleOrderColumn = """
        CASE WHEN c."ParentId" IS NULL THEN 0
             WHEN p."ParentId" IS NULL THEN c."DisplayOrder"
             ELSE p."DisplayOrder" END
        """;

    /// <summary>The row's own order at the last level, 0 for shallower rows.</summary>
    private const string LeafOrderColumn = """CASE WHEN g."Id" IS NULL THEN 0 ELSE c."DisplayOrder" END""";

    #endregion

    /// <summary>
    /// The admin list of categories: one row per live category, joined to its parent for the name
    /// column and to the two levels above it for the computed ones. Paging, sorting and both record
    /// counts come from the shared data-table engine, so a new column is one whitelist entry here.
    /// Rows come out in tree order — a parent immediately followed by its children — because that is
    /// the only order a drag &amp; drop list of siblings can be read in.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {catalog}."Categories" c
            LEFT JOIN {catalog}."Categories" p ON p."Id" = c."ParentId" AND p."IsDeleted" = FALSE
            LEFT JOIN {catalog}."Categories" g ON g."Id" = p."ParentId" AND g."IsDeleted" = FALSE
            """,
        baseWhere: """c."IsDeleted" = FALSE""",
        defaultOrder: $"""{RootOrderColumn} ASC, {MiddleOrderColumn} ASC, {LeafOrderColumn} ASC, c."Id" ASC""",
        columns: SqlDataTableSpec.WithExtras(
            SqlDataTableSpec.ColumnsOf<CategoryEntity>("c"),
            [
                ("ParentName", """p."Name" """, typeof(string)),
                ("Depth", DepthColumn, typeof(int)),
            ]),
        searchColumns: [nameof(CategoryEntity.Name), nameof(CategoryEntity.LatinName)]);

    public Task<DataTableResult<CategoryListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<CategoryListItemDto>(dataSource, filter, conditions, ct);
}
