using Whyland.Infrastructure.Persistence.Extensions;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Content.Faq;
using FaqEntity = Whyland.Domain.Entities.Content.Faq;

namespace Whyland.Infrastructure.Persistence.Repositories.Content.Faq;

/// <summary>
/// Represents a Dapper-based query repository for the <c>Faq</c> aggregate.
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public class FaqQueryRepository(NpgsqlDataSource dataSource) : IFaqQueryRepository
{
    const string content = SchemaConstants.Content;

    /// <summary>
    /// The admin list of FAQs. Paging, sorting and both record counts are produced by the shared
    /// data-table engine, so a new column is one whitelist entry here and never a new query.
    /// A free-text search is matched across the title and the answer, which is what the box
    /// under the table promises.
    /// </summary>
    private static readonly SqlDataTableSpec TableSpec = new(
        from: $"""
            {content}."Faqs" f
            """,
        baseWhere: """f."IsDeleted" = FALSE""",
        defaultOrder: """f."DisplayOrder" ASC, f."Id" ASC""",
        columns: SqlDataTableSpec.ColumnsOf<FaqEntity>("f"),
        searchColumns: ["Title", "Description"]);

    public Task<IReadOnlyList<FaqPublicItemDto>> GetActiveListAsync(CancellationToken ct = default)
        => DapperFluentQueryBuilder<FaqEntity>.Create(
                $"{content}.Faqs", x => x.Title, x => x.Description)
            .Where(x => x.IsActive, true)
            .Sortable(x => x.DisplayOrder)
            .OrderBy(nameof(FaqEntity.DisplayOrder))
            .TieBreakBy(x => x.Id)
            .GetAllAsync<FaqPublicItemDto>(dataSource, ct);

    public Task<FaqDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => DapperFluentQueryBuilder<FaqEntity>.Create(
                $"{content}.Faqs", x => x.Id, x => x.Title, x => x.Description,
                x => x.DisplayOrder, x => x.IsActive, x => x.CreatedAt, x => x.UpdatedAt)
            .Where(x => x.Id, id)
            .GetFirstOrDefaultAsync<FaqDto>(dataSource, ct);

    public Task<DataTableResult<FaqListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        CancellationToken ct = default)
        => TableSpec.GetDataTableAsync<FaqListItemDto>(dataSource, filter, null, ct);
}
