using Dapper;
using Whyland.Shared.Dtos.Content.Faq;

namespace Whyland.Infrastructure.Persistence.Repositories.Content.Faq;

/// <summary>
/// Represents a Dapper-based query repository for the <c>Faq</c> aggregate.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public class FaqQueryRepository(NpgsqlDataSource dataSource) : IFaqQueryRepository
{
    /// <summary>
    /// A whitelist of sortable columns that maps client-provided sort keys to
    /// their corresponding quoted SQL identifiers.
    /// </summary>
    public static readonly Dictionary<string, string> SortableColumns = 
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"]           = "\"Id\"",
            ["title"]        = "\"Title\"",
            ["displayorder"] = "\"DisplayOrder\"",
            ["isactive"]     = "\"IsActive\"",
            ["createdat"]    = "\"CreatedAt\"",
            ["updatedat"]    = "\"UpdatedAt\"",
        };

    public async Task<IReadOnlyList<FaqPublicItemDto>> GetActiveListAsync(CancellationToken ct = default)
    {
        const string sql = """
            SELECT "Title", "Description"
            FROM content."Faqs"
            WHERE "IsDeleted" = FALSE
              AND "IsActive"  = TRUE
            ORDER BY "DisplayOrder" ASC, "Id" ASC;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<FaqPublicItemDto>(
            new CommandDefinition(sql, cancellationToken: ct));

        return rows.AsList();
    }

    public async Task<FaqDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        const string sql = """
            SELECT "Id", "Title", "Description", "DisplayOrder", "IsActive", "CreatedAt", "UpdatedAt"
            FROM content."Faqs"
            WHERE "Id" = @Id
              AND "IsDeleted" = FALSE;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);

        return await connection.QueryFirstOrDefaultAsync<FaqDto>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }
    
    public async Task<DataTableResult<FaqListItemDto>> GetForDataTableAsync(DataTableFilter filter, CancellationToken ct = default)
    {
        const string baseWhere = "\"IsDeleted\" = FALSE";

        var parameters = new DynamicParameters();
        string searchWhere = baseWhere;

        if (!string.IsNullOrEmpty(filter.SearchValue))
        {
            searchWhere +=
                " AND (\"Title\" ILIKE @Search OR \"Description\" ILIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchValue.Trim()}%");
        }

        parameters.Add("PageSize",
            int.TryParse(filter.Length, out var length) && length > 0
                ? Math.Min(length, 10000)   // Hard cap guards one-shot "all rows" page loads.
                : filter.PageSize > 0 ? filter.PageSize : 10);
        parameters.Add("Skip",
            int.TryParse(filter.Start, out var start) && start >= 0
                ? start
                : filter.Skip >= 0 ? filter.Skip : 0);

        var sortColumn =
            SortableColumns.TryGetValue(filter.SortColumn ?? "createdAt", out var column)
                ? column : "\"CreatedAt\"";
        var direction = string.Equals(
            filter.SortColumnDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC" : "ASC";

        var sql = $"""
            SELECT COUNT(*) FROM content."Faqs" WHERE {baseWhere};

            SELECT COUNT(*) FROM content."Faqs" WHERE {searchWhere};

            SELECT "Id", "Title", "Description", "DisplayOrder", "IsActive", "CreatedAt"
            FROM content."Faqs"
            WHERE {searchWhere}
            ORDER BY {sortColumn} {direction}
            LIMIT @PageSize OFFSET @Skip;
            """;

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, parameters, cancellationToken: ct));

        var recordsTotal = await multi.ReadFirstAsync<int>();
        var recordsFiltered = await multi.ReadFirstAsync<int>();
        var rows = (await multi.ReadAsync<FaqListItemDto>()).ToList();

        return new DataTableResult<FaqListItemDto>
        {
            Draw = filter.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
            Data = rows,
        };
    }
}
