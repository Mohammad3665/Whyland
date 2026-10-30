using System.Data;
using System.Text;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Everything the data-table engine needs for one list: the FROM clause, the predicate the list
/// starts from, the whitelist of client-facing columns, the fallback order and whether the rows
/// are distinct.
/// <para>
/// It is produced two ways and read one way: the spec extension wraps a declarative
/// <see cref="SqlDataTableSpec"/>, and the builder's <c>ToDataTableSource</c> wraps the
/// registrations of a typed builder. Both go through <see cref="SqlDataTableQuery"/>, so paging,
/// searching, sorting and value conversion behave identically for every table in the application.
/// </para>
/// </summary>
/// <param name="From">The table plus its alias and any join, without the FROM keyword.</param>
/// <param name="Where">The predicate the list starts from, plus any condition the code fixed.</param>
/// <param name="Columns">The whitelist, keyed by property name and matched case-insensitively.</param>
/// <param name="DefaultOrder">The ORDER BY used when the client did not pick a column.</param>
/// <param name="Distinct">Whether rows are de-duplicated, which the counts must mirror.</param>
/// <param name="searchColumns">The ORed group a fieldless free-text search reads.</param>
public sealed class SqlDataTableSource(
    string from,
    SqlConditionWriter where,
    IReadOnlyDictionary<string, SqlColumn> columns,
    string defaultOrder,
    bool distinct = false,
    IEnumerable<SqlColumn>? searchColumns = null)
{
    /// <summary>The table and its joins, without the FROM keyword.</summary>
    public string From { get; } = from;

    /// <summary>The base predicate and any code-supplied condition.</summary>
    public SqlConditionWriter Where { get; } = where;

    /// <summary>The usable columns.</summary>
    public IReadOnlyDictionary<string, SqlColumn> Columns { get; } = columns;

    /// <summary>The fallback ORDER BY.</summary>
    public string DefaultOrder { get; } = defaultOrder;

    /// <summary>Whether the row set is DISTINCT.</summary>
    public bool Distinct { get; } = distinct;

    /// <summary>
    /// The columns a search is ORed across when the request names no field. Empty means the list
    /// searches its first text column.
    /// </summary>
    public IReadOnlyList<SqlColumn> SearchColumns { get; } = [.. searchColumns ?? []];

    /// <summary>Resolves a client-supplied column name, or null when it is not whitelisted.</summary>
    public SqlColumn? Find(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? null
            : Columns.TryGetValue(name, out var column) ? column : null;

    /// <summary>First text column, used when the client searches without naming a field.</summary>
    public SqlColumn? DefaultSearchColumn()
        => Columns.Values.FirstOrDefault(x => x.ClrType == typeof(string));

    /// <summary>
    /// Turns the row type's properties into the SELECT list. A member with no whitelisted column
    /// is a mapping mistake, so it fails with the name instead of returning a silent null.
    /// </summary>
    public string BuildSelect<TResult>()
    {
        var columns = new List<string>();

        foreach (var property in typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var column = Find(property.Name)
                ?? throw new InvalidOperationException(
                    $"Row type '{typeof(TResult).Name}' has a member '{property.Name}' with no mapped column.");

            columns.Add(column.Expression + " AS \"" + property.Name + "\"");
        }

        return string.Join(", ", columns);
    }

    /// <summary>
    /// Builds the ORDER BY from the requested column, falling back to the source default. The key
    /// column is appended as a tiebreaker so two pages never show the same row twice.
    /// </summary>
    public string BuildOrder(DataTableFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var column = Find(filter.SortColumn);
        if (column is null)
            return DefaultOrder;

        var direction = string.Equals(filter.SortColumnDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        var key = Find("Id");
        var tiebreaker = key is not null && !string.Equals(key.Expression, column.Expression, StringComparison.Ordinal)
            ? ", " + key.Expression
            : string.Empty;

        return column.Expression + " " + direction + tiebreaker;
    }

    /// <summary>One count statement, de-duplicated the same way the rows are when the list is distinct.</summary>
    private string BuildCount(string whereSql, string selectList)
        => Distinct
            ? $"SELECT COUNT(*) FROM (SELECT DISTINCT {selectList} FROM {From} WHERE {whereSql}) __distinct_rows;"
            : $"SELECT COUNT(*) FROM {From} WHERE {whereSql};";

    /// <summary>
    /// The full three-statement batch: the unfiltered total, the filtered total and the page.
    /// </summary>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="selectList">The SELECT list of the row statement.</param>
    /// <returns>The batch text, with the page parameters added to the cloned writer.</returns>
    internal (string Sql, SqlConditionWriter Where) BuildBatch(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions,
        string selectList)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var where = Where.Clone();

        var search = (filter.SearchValue ?? string.Empty).Trim();
        if (search.Length > 0)
        {
            // A named field wins; otherwise the list decides for itself - the columns it declared
            // as its search group, or, with none declared, its first text column.
            var named = Find(filter.SearchField);
            if (named is not null)
                where.ApplyText(named, FilterOperator.Contains, search);
            else if (SearchColumns.Count > 0)
                where.ApplyTextAcross(SearchColumns, FilterOperator.Contains, search);
            else if (DefaultSearchColumn() is { } fallback)
                where.ApplyText(fallback, FilterOperator.Contains, search);
        }

        if (conditions is not null)
        {
            foreach (var condition in conditions)
                where.Apply(Find(condition.Property), condition.Operator, condition.Value);
        }

        var (take, skip) = SqlPaging.From(filter);
        where.Parameters.Add("__limit", take);
        where.Parameters.Add("__offset", skip);

        var whereSql = where.Sql;
        var order = BuildOrder(filter);

        var builder = new StringBuilder();
        builder.AppendLine(BuildCount(where.BaseSql, selectList));
        builder.AppendLine(BuildCount(whereSql, selectList));
        builder.Append("SELECT ").Append(Distinct ? "DISTINCT " : string.Empty).AppendLine(selectList);
        builder.Append("FROM ").AppendLine(From);

        if (whereSql.Length > 0)
            builder.Append("WHERE ").AppendLine(whereSql);

        if (order.Length > 0)
            builder.Append("ORDER BY ").AppendLine(order);

        builder.Append("LIMIT @__limit OFFSET @__offset;");

        return (builder.ToString(), where);
    }
}

/// <summary>Runs a <see cref="SqlDataTableSource"/>.</summary>
public static class SqlDataTableQuery
{
    /// <summary>Reads one page of rows together with the total and the filtered record counts.</summary>
    /// <typeparam name="TResult">The row DTO; every one of its properties must be a whitelisted column.</typeparam>
    /// <param name="source">The list definition.</param>
    /// <param name="connection">An open connection to run on.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static async Task<DataTableResult<TResult>> RunAsync<TResult>(
        this SqlDataTableSource source,
        IDbConnection connection,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(connection);

        var (sql, where) = source.BuildBatch(filter, conditions, source.BuildSelect<TResult>());

        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, where.Parameters, cancellationToken: ct));

        return new DataTableResult<TResult>
        {
            Draw = filter.Draw,
            RecordsTotal = await multi.ReadFirstAsync<int>(),
            RecordsFiltered = await multi.ReadFirstAsync<int>(),
            Data = (await multi.ReadAsync<TResult>()).AsList(),
        };
    }

    /// <summary>Reads one page over a pooled data source, opening and closing the connection here.</summary>
    /// <typeparam name="TResult">The row DTO; every one of its properties must be a whitelisted column.</typeparam>
    /// <param name="source">The list definition.</param>
    /// <param name="dataSource">The source the connection is opened from.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static async Task<DataTableResult<TResult>> RunAsync<TResult>(
        this SqlDataTableSource source,
        NpgsqlDataSource dataSource,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(dataSource);

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await source.RunAsync<TResult>(connection, filter, conditions, ct);
    }
}

/// <summary>
/// The declarative entry point: a <see cref="SqlDataTableSpec"/> describes the list and this
/// reads it through the same engine as the typed builder.
/// </summary>
public static class SqlDataTableSpecExtensions
{
    /// <summary>Reads one page of the list the spec describes, with both record counts.</summary>
    /// <typeparam name="TResult">The row DTO; every one of its properties must be a whitelisted column.</typeparam>
    /// <param name="spec">The table, joins and column whitelist of the list.</param>
    /// <param name="dataSource">The source the connection is opened from.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static Task<DataTableResult<TResult>> GetDataTableAsync<TResult>(
        this SqlDataTableSpec spec,
        NpgsqlDataSource dataSource,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where TResult : class
        => spec.ToDataTableSource().RunAsync<TResult>(dataSource, filter, conditions, ct);

    /// <summary>Reads one page of the list the spec describes on a connection the caller owns.</summary>
    /// <typeparam name="TResult">The row DTO; every one of its properties must be a whitelisted column.</typeparam>
    /// <param name="spec">The table, joins and column whitelist of the list.</param>
    /// <param name="connection">An open connection to run on.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static Task<DataTableResult<TResult>> GetDataTableAsync<TResult>(
        this SqlDataTableSpec spec,
        IDbConnection connection,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where TResult : class
        => spec.ToDataTableSource().RunAsync<TResult>(connection, filter, conditions, ct);

    /// <summary>Wraps the spec as an engine source.</summary>
    private static SqlDataTableSource ToDataTableSource(this SqlDataTableSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        return new SqlDataTableSource(
            spec.From,
            new SqlConditionWriter(spec.BaseWhere),
            spec.Columns,
            spec.DefaultOrder,
            searchColumns: spec.SearchColumns);
    }
}
