using System.Data;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Runs what <see cref="DapperFluentQueryBuilder{TEntity}"/> built, so a repository reads
/// <c>builder.GetAllAsync&lt;Dto&gt;(dataSource, ct)</c> instead of assembling a
/// <see cref="CommandDefinition"/> itself. Connections are opened and closed here when the caller
/// hands over a data source; a caller that already owns a connection passes that instead, which is
/// what a transactional read needs.
/// </summary>
public static class DapperFluentQueryExtensions
{
    /// <summary>Reads every matching row.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Finished builder.</param>
    /// <param name="connection">Open connection to run on.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<IReadOnlyList<T>> GetAllAsync<T>(
        this IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        CancellationToken ct = default)
    {
        var command = Prepare(builder, connection, ct);
        var items = await connection.QueryAsync<T>(command);
        return items.ToList();
    }

    /// <summary>Reads the first matching row, or null when nothing matches.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Finished builder.</param>
    /// <param name="connection">Open connection to run on.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<T?> GetFirstOrDefaultAsync<T>(
        this IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        CancellationToken ct = default)
        where T : class
    {
        var command = Prepare(builder, connection, ct);
        return await connection.QueryFirstOrDefaultAsync<T>(command);
    }

    /// <summary>Reads the only matching row; null when nothing matches, an error when several do.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Finished builder.</param>
    /// <param name="connection">Open connection to run on.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<T?> GetSingleOrDefaultAsync<T>(
        this IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        CancellationToken ct = default)
        where T : class
    {
        var command = Prepare(builder, connection, ct);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    /// <summary>Reads one page together with the total number of matching rows.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Builder with its filters, registered sorts and optional tiebreaker.</param>
    /// <param name="connection">Open connection to run on.</param>
    /// <param name="request">Page, page size and requested sort.</param>
    /// <param name="defaultSort">Sort key used when the request names none or an unregistered one.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<PagedResult<T>> GetPagedAsync<T>(
        this IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        QueryRequest request,
        string defaultSort = "Id",
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(request);

        var query = builder.BuildPaged(request, defaultSort);
        var command = new CommandDefinition(query.Sql, query.Parameters, cancellationToken: ct);

        await using var reader = await connection.QueryMultipleAsync(command);

        var totalCount = await reader.ReadSingleAsync<long>();
        var items = (await reader.ReadAsync<T>()).ToList();

        return new PagedResult<T>(items, query.Page, query.PageSize, totalCount);
    }

    /// <summary>Reads every matching row over a pooled data source.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Finished builder.</param>
    /// <param name="dataSource">Source the connection is opened from.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<IReadOnlyList<T>> GetAllAsync<T>(
        this IDapperFluentQueryBuilder builder,
        NpgsqlDataSource dataSource,
        CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(dataSource, ct);
        return await builder.GetAllAsync<T>(connection, ct);
    }

    /// <summary>Reads the first matching row over a pooled data source, or null.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Finished builder.</param>
    /// <param name="dataSource">Source the connection is opened from.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<T?> GetFirstOrDefaultAsync<T>(
        this IDapperFluentQueryBuilder builder,
        NpgsqlDataSource dataSource,
        CancellationToken ct = default)
        where T : class
    {
        await using var connection = await OpenAsync(dataSource, ct);
        return await builder.GetFirstOrDefaultAsync<T>(connection, ct);
    }

    /// <summary>Reads one page over a pooled data source, with the total row count.</summary>
    /// <typeparam name="T">Row type the output columns map onto.</typeparam>
    /// <param name="builder">Builder with its filters and registered sorts.</param>
    /// <param name="dataSource">Source the connection is opened from.</param>
    /// <param name="request">Page, page size and requested sort.</param>
    /// <param name="defaultSort">Sort key used when the request names none or an unregistered one.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task<PagedResult<T>> GetPagedAsync<T>(
        this IDapperFluentQueryBuilder builder,
        NpgsqlDataSource dataSource,
        QueryRequest request,
        string defaultSort = "Id",
        CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(dataSource, ct);
        return await builder.GetPagedAsync<T>(connection, request, defaultSort, ct);
    }

    /// <summary>
    /// Reads one page of a server-side data table: the unfiltered total, the filtered total and
    /// the rows, with the search, the sort and the field conditions resolved by the shared engine
    /// against the columns this builder registered.
    /// </summary>
    /// <typeparam name="T">Row type; every one of its properties must be a registered column.</typeparam>
    /// <param name="builder">Builder carrying the list definition.</param>
    /// <param name="connection">Open connection to run on.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static Task<DataTableResult<T>> GetDataTableAsync<T>(
        this IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ToDataTableSource().RunAsync<T>(connection, filter, conditions, ct);
    }

    /// <summary>Reads one page of a server-side data table over a pooled data source.</summary>
    /// <typeparam name="T">Row type; every one of its properties must be a registered column.</typeparam>
    /// <param name="builder">Builder carrying the list definition.</param>
    /// <param name="dataSource">Source the connection is opened from.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static Task<DataTableResult<T>> GetDataTableAsync<T>(
        this IDapperFluentQueryBuilder builder,
        NpgsqlDataSource dataSource,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.ToDataTableSource().RunAsync<T>(dataSource, filter, conditions, ct);
    }

    private static async Task<NpgsqlConnection> OpenAsync(NpgsqlDataSource dataSource, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        return await dataSource.OpenConnectionAsync(ct);
    }

    private static CommandDefinition Prepare(
        IDapperFluentQueryBuilder builder,
        IDbConnection connection,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(connection);

        var query = builder.Build();
        return new CommandDefinition(query.Sql, query.Parameters, cancellationToken: ct);
    }
}
