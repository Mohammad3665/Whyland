using Dapper;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// A count statement plus the page statement, as one batch, with the page the caller asked for.
/// </summary>
/// <param name="Sql">Two statements: the total count, then the page rows.</param>
/// <param name="Parameters">The filter values plus the bound page size and offset.</param>
/// <param name="Page">The one-based page number that was resolved.</param>
/// <param name="PageSize">The page size that was resolved.</param>
public sealed record PagedSqlQuery(string Sql, DynamicParameters Parameters, int Page, int PageSize);
