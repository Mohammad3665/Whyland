using System.Globalization;
using System.Reflection;
using System.Text;
using Dapper;
using Whyland.Shared.Enums;
using Whyland.Shared.Filters;

namespace Whyland.Infrastructure.Persistence.Extensions;

/// <summary>
/// Runs a <see cref="DataTableFilter"/> plus generic <see cref="FilterCondition"/>s against
/// PostgreSQL through Dapper, so query repositories stay on Dapper while the page still gets
/// server-side paging, sorting, searching and filtering.
/// The SQL is assembled from a <see cref="SqlDataTableSpec"/>: column identifiers come from its
/// whitelist and values only ever arrive as parameters, so no request field can reach the
/// statement as text. Unknown properties and unparsable values are dropped, which keeps a
/// mistyped filter from turning into a database error.
/// </summary>
public static class DapperDataTableExtensions
{
    private const int DefaultPageSize = 10;

    /// <summary>Hard cap, so a hand-made "length=999999" cannot pull a whole table.</summary>
    private const int MaxPageSize = 1000;

    /// <summary>Reads one page of rows together with the total and the filtered record counts.</summary>
    /// <typeparam name="TResult">The row DTO; every one of its properties must be a whitelisted column.</typeparam>
    /// <param name="dataSource">The pooled Npgsql data source the read runs on.</param>
    /// <param name="spec">The table, joins and column whitelist of the list.</param>
    /// <param name="filter">Paging, sorting and search parameters sent by the table.</param>
    /// <param name="conditions">Extra field/operator/value conditions, ANDed with the search.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public static async Task<DataTableResult<TResult>> ToDataTableAsync<TResult>(
        this NpgsqlDataSource dataSource,
        SqlDataTableSpec spec,
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default)
        where TResult : class
    {
        var builder = new SqlWhereBuilder(spec.BaseWhere);

        var search = (filter.SearchValue ?? string.Empty).Trim();
        if (search.Length > 0)
        {
            // The search names its field; with no usable name it falls back to the first text
            // column, exactly like QueryRequestMappingExtensions does.
            var column = spec.Find(filter.SearchField) ?? spec.DefaultSearchColumn();
            if (column is not null) builder.ApplyText(column, FilterOperator.Contains, search);
        }

        if (conditions is not null)
        {
            foreach (var condition in conditions)
                builder.Apply(spec.Find(condition.Property), condition.Operator, condition.Value);
        }

        var (take, skip) = ReadPaging(filter);
        builder.Parameters.Add("Limit", take);
        builder.Parameters.Add("Offset", skip);

        var sql = string.Join(Environment.NewLine,
            ["SELECT COUNT(*) FROM " + spec.From + " WHERE " + spec.BaseWhere + ";",
             "SELECT COUNT(*) FROM " + spec.From + " WHERE " + builder.Sql + ";",
             "SELECT " + BuildSelect<TResult>(spec),
             "FROM " + spec.From,
             "WHERE " + builder.Sql,
             "ORDER BY " + BuildOrder(spec, filter),
             "LIMIT @Limit OFFSET @Offset;"]);

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql, builder.Parameters, cancellationToken: ct));

        return new DataTableResult<TResult>
        {
            Draw = filter.Draw,
            RecordsTotal = await multi.ReadFirstAsync<int>(),
            RecordsFiltered = await multi.ReadFirstAsync<int>(),
            Data = (await multi.ReadAsync<TResult>()).AsList(),
        };
    }

    /// <summary>
    /// Turns the DTO properties into the SELECT list. A DTO member with no whitelisted column is
    /// a mapping mistake, so it fails with the name instead of returning a silent null.
    /// </summary>
    private static string BuildSelect<TResult>(SqlDataTableSpec spec)
    {
        var columns = new List<string>();

        foreach (var property in typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var column = spec.Find(property.Name)
                ?? throw new InvalidOperationException(
                    $"Row type '{typeof(TResult).Name}' has a member '{property.Name}' with no mapped column.");

            columns.Add(column.Expression + " AS \"" + property.Name + "\"");
        }

        return string.Join(", ", columns);
    }

    /// <summary>
    /// Builds the ORDER BY from the requested column, falling back to the spec default. The key
    /// column is appended as a tiebreaker so two pages never show the same row twice.
    /// </summary>
    private static string BuildOrder(SqlDataTableSpec spec, DataTableFilter filter)
    {
        var column = spec.Find(filter.SortColumn);
        if (column is null)
            return spec.DefaultOrder;

        var direction = string.Equals(filter.SortColumnDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "DESC"
            : "ASC";

        var key = spec.Find("Id");
        var tiebreaker = key is not null && !ReferenceEquals(key, column) ? ", " + key.Expression : string.Empty;

        return column.Expression + " " + direction + tiebreaker;
    }

    /// <summary>Reads the page size and offset from the DataTables strings, capping the size.</summary>
    private static (int Take, int Skip) ReadPaging(DataTableFilter filter)
    {
        var take = int.TryParse(filter.Length, NumberStyles.Integer, CultureInfo.InvariantCulture, out var length)
                   && length > 0
            ? length
            : filter.PageSize > 0 ? filter.PageSize : DefaultPageSize;

        if (take > MaxPageSize) take = MaxPageSize;

        var skip = int.TryParse(filter.Start, NumberStyles.Integer, CultureInfo.InvariantCulture, out var start)
                   && start >= 0
            ? start
            : filter.Skip > 0 ? filter.Skip : 0;

        return (take, skip);
    }

    /// <summary>
    /// Accumulates WHERE fragments and their parameters, naming each parameter after its own
    /// position so repeated columns never collide.
    /// </summary>
    private sealed class SqlWhereBuilder(string baseWhere)
    {
        private readonly StringBuilder _sql = new(baseWhere);
        private int _index;

        /// <summary>The parameters collected for the statement.</summary>
        public DynamicParameters Parameters { get; } = new();

        /// <summary>The finished WHERE clause.</summary>
        public string Sql => _sql.ToString();

        /// <summary>Adds a text comparison as a case-insensitive LIKE with escaped wildcards.</summary>
        public void ApplyText(SqlColumn column, FilterOperator operatorName, string value)
        {
            if (column.ClrType != typeof(string))
                return;

            var escaped = value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = operatorName switch
            {
                FilterOperator.StartsWith => escaped + "%",
                FilterOperator.EndsWith => "%" + escaped,
                _ => "%" + escaped + "%",
            };

            var name = NextName();
            Parameters.Add(name, pattern);
            _sql.Append(" AND ").Append(column.Expression).Append(" ILIKE @").Append(name);
        }

        /// <summary>
        /// Adds one field/operator/value condition. An unmapped property, an empty value or a
        /// value the column type cannot hold leaves the condition out instead of failing.
        /// </summary>
        public void Apply(SqlColumn? column, FilterOperator operatorName, string? rawValue)
        {
            if (column is null)
                return;

            var value = (rawValue ?? string.Empty).Trim();

            if (operatorName is FilterOperator.Contains
                or FilterOperator.StartsWith
                or FilterOperator.EndsWith)
            {
                if (value.Length > 0) ApplyText(column, operatorName, value);
                return;
            }

            if (operatorName == FilterOperator.In)
            {
                var names = new List<string>();

                foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries
                                               | StringSplitOptions.TrimEntries))
                {
                    if (!TryConvert(column, part, out var item)) return;

                    var name = NextName();
                    Parameters.Add(name, item);
                    names.Add("@" + name);
                }

                if (names.Count > 0)
                    _sql.Append(" AND ").Append(column.Expression)
                        .Append(" IN (").Append(string.Join(", ", names)).Append(')');

                return;
            }

            if (!TryConvert(column, value, out var converted))
                return;

            var paramName = NextName();
            Parameters.Add(paramName, converted);
            _sql.Append(" AND ").Append(column.Expression).Append(' ')
                .Append(OperatorSql(operatorName)).Append(" @").Append(paramName);
        }

        private string NextName() => "f" + _index++;

        private static string OperatorSql(FilterOperator operatorName) => operatorName switch
        {
            FilterOperator.NotEquals => "<>",
            FilterOperator.GreaterThan => ">",
            FilterOperator.GreaterThanOrEqual => ">=",
            FilterOperator.LessThan => "<",
            FilterOperator.LessThanOrEqual => "<=",
            _ => "=",
        };

        /// <summary>
        /// Converts one client value to something Npgsql can send for this column type. Anything
        /// unparsable is reported as unsupported so the caller drops the condition.
        /// </summary>
        private static bool TryConvert(SqlColumn column, string value, out object converted)
        {
            converted = value;
            var type = column.ClrType;

            if (type == typeof(string))
                return value.Length > 0;

            if (type == typeof(bool))
            {
                if (!bool.TryParse(value, out var flag)) return false;
                converted = flag;
                return true;
            }

            if (type == typeof(Guid))
            {
                if (!Guid.TryParse(value, out var id)) return false;
                converted = id;
                return true;
            }

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var moment))
                    return false;

                converted = DateTime.SpecifyKind(moment, DateTimeKind.Utc);
                return true;
            }

            if (type == typeof(DateOnly))
            {
                if (!DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                    return false;

                converted = day;
                return true;
            }

            if (type == typeof(TimeOnly))
            {
                if (!TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var clock))
                    return false;

                converted = clock;
                return true;
            }

            if (type.IsEnum)
            {
                if (!Enum.TryParse(type, value, ignoreCase: true, out var parsed) || parsed is null)
                    return false;

                converted = Convert.ChangeType(parsed, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
                return true;
            }

            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
            {
                if (!decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                    return false;

                converted = number;
                return true;
            }

            if (IsInteger(type))
            {
                if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var whole))
                    return false;

                converted = whole;
                return true;
            }

            return false;
        }

        private static bool IsInteger(Type type)
            => type == typeof(byte) || type == typeof(short) || type == typeof(int) || type == typeof(long)
               || type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
    }
}
