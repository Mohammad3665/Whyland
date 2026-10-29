namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// What a built statement needs to expose, so the execution helpers do not depend on the entity
/// type the statement was built from.
/// </summary>
public interface IDapperFluentQueryBuilder
{
    /// <summary>Builds the statement without running it.</summary>
    DapperSqlQuery Build();

    /// <summary>Builds the count-plus-page batch without running it.</summary>
    /// <param name="request">Page, page size, sort field and sort direction.</param>
    /// <param name="defaultSort">Sort key used when the request names none.</param>
    PagedSqlQuery BuildPaged(QueryRequest request, string defaultSort = "Id");

    /// <summary>
    /// Hands the built statement to the data-table engine, so the same list can be paged,
    /// searched, sorted and filtered by a server-side table without a second definition of it.
    /// </summary>
    SqlDataTableSource ToDataTableSource();
}
