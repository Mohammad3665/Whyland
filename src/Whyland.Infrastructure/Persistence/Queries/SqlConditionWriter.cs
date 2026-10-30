using System.Globalization;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Assembles one WHERE clause and the parameters it binds, and is the only place in the
/// infrastructure layer where the meaning of a <see cref="FilterOperator"/> is decided.
/// <para>
/// The typed <see cref="DapperFluentQueryBuilder{TEntity}"/> and the declarative
/// <see cref="SqlDataTableSpec"/> both write through this class, so the escaping of a LIKE
/// pattern, the expansion of <see cref="FilterOperator.In"/> and the conversion of a client
/// string into a column type exist once instead of twice.
/// </para>
/// <para>
/// Column identifiers arrive already quoted, taken from a whitelist or from a property
/// expression - never from the request - and every value leaves as a parameter, so no request
/// field can reach the statement as text. An unmapped column or an unparsable value drops the
/// condition rather than turning into a database error.
/// </para>
/// </summary>
public sealed class SqlConditionWriter
{
    private readonly List<string> _fragments = [];
    private int _index;

    /// <summary>Creates a writer; the base predicate is the fragment every query starts from.</summary>
    /// <param name="baseWhere">Already-quoted predicate such as <c>"t0"."IsDeleted" = FALSE</c>, or null.</param>
    public SqlConditionWriter(string? baseWhere = null) => BaseWhere = baseWhere;

    /// <summary>The predicate every query starts from: the soft-delete guard, or a spec's base filter.</summary>
    public string? BaseWhere { get; set; }

    /// <summary>The parameters collected for the statement, named by their own position.</summary>
    public DynamicParameters Parameters { get; } = new();

    /// <summary>The appended predicates, in the order they were added.</summary>
    public IReadOnlyList<string> Fragments => _fragments;

    /// <summary>The base predicate with every appended fragment: the body of the filtered WHERE.</summary>
    public string Sql
        => string.IsNullOrWhiteSpace(BaseWhere)
            ? _fragments.Count == 0 ? string.Empty : string.Join(" AND ", _fragments)
            : _fragments.Count == 0 ? BaseWhere! : BaseWhere + " AND " + string.Join(" AND ", _fragments);

    /// <summary>Only the base predicate, which is what an unfiltered total count reads.</summary>
    public string BaseSql => string.IsNullOrWhiteSpace(BaseWhere) ? "TRUE" : BaseWhere!;

    /// <summary>A copy with the same base, the same fragments and the same parameter values.</summary>
    public SqlConditionWriter Clone()
    {
        var clone = new SqlConditionWriter(BaseWhere);
        clone._index = _index;
        clone._fragments.AddRange(_fragments);

        foreach (var name in Parameters.ParameterNames)
            clone.Parameters.Add(name, Parameters.Get<object?>(name));

        return clone;
    }

    /// <summary>Appends an already-written predicate that needs no parameter.</summary>
    public void AddRaw(string predicate) => _fragments.Add("(" + predicate + ")");

    /// <summary>Appends an IS NULL / IS NOT NULL test.</summary>
    /// <param name="column">Quoted column expression.</param>
    /// <param name="negate">IS NOT NULL when true.</param>
    public void AddNullTest(string column, bool negate)
        => _fragments.Add($"({column} {(negate ? "IS NOT NULL" : "IS NULL")})");

    /// <summary>Appends one comparison and binds its value.</summary>
    /// <param name="column">Quoted column expression.</param>
    /// <param name="comparison">SQL operator, e.g. <c>=</c> or <c>&lt;&gt;</c>.</param>
    /// <param name="value">Value to bind; enums and unspecified-kind dates are normalised first.</param>
    public void AddComparison(string column, string comparison, object? value)
    {
        var name = NextName();
        Parameters.Add(name, Normalize(value));
        _fragments.Add($"({column} {comparison} @{name})");
    }

    /// <summary>Appends one set-membership test with a parameter per value.</summary>
    /// <param name="column">Quoted column expression.</param>
    /// <param name="values">Values to bind; an empty set appends nothing.</param>
    public void AddIn(string column, IEnumerable<object?> values)
    {
        var names = new List<string>();

        foreach (var value in values)
        {
            var name = NextName();
            Parameters.Add(name, Normalize(value));
            names.Add("@" + name);
        }

        if (names.Count > 0)
            _fragments.Add($"({column} IN ({string.Join(", ", names)}))");
    }

    /// <summary>Appends a case-insensitive LIKE, with the wildcards in the value escaped.</summary>
    /// <param name="column">Whitelisted column; a non-text column is ignored.</param>
    /// <param name="operatorName">Only the three text operators are read from it.</param>
    /// <param name="value">Text to look for.</param>
    public void ApplyText(SqlColumn column, FilterOperator operatorName, string value)
    {
        if (column.ClrType != typeof(string))
            return;

        var name = NextName();
        Parameters.Add(name, LikePattern(value, operatorName));
        _fragments.Add($"({column.Expression} ILIKE @{name})");
    }

    /// <summary>
    /// Appends one case-insensitive LIKE shared by several columns, which is how a free-text box
    /// searches a whole row instead of the one field its name happens to match.
    /// </summary>
    /// <param name="columns">Text columns to look in; anything else is dropped.</param>
    /// <param name="operatorName">Only the three text operators are read from it.</param>
    /// <param name="value">Text to look for.</param>
    public void ApplyTextAcross(IReadOnlyList<SqlColumn> columns, FilterOperator operatorName, string value)
    {
        var text = columns.Where(x => x.ClrType == typeof(string)).ToList();
        if (text.Count == 0)
            return;

        var name = NextName();
        Parameters.Add(name, LikePattern(value, operatorName));
        _fragments.Add("(" + string.Join(" OR ", text.Select(x => $"{x.Expression} ILIKE @{name}")) + ")");
    }

    /// <summary>The LIKE pattern an operator implies, with the wildcards inside the value escaped.</summary>
    private static string LikePattern(string value, FilterOperator operatorName)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        return operatorName switch
        {
            FilterOperator.StartsWith => escaped + "%",
            FilterOperator.EndsWith => "%" + escaped,
            _ => "%" + escaped + "%",
        };
    }

    /// <summary>
    /// Appends one field/operator/value condition written the way a request writes it: the value
    /// arrives as text and is converted to the column type first.
    /// </summary>
    /// <param name="column">Whitelisted column, or null when the client named something unmapped.</param>
    /// <param name="operatorName">How the value is compared.</param>
    /// <param name="rawValue">The text sent by the client.</param>
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
                _fragments.Add($"({column.Expression} IN ({string.Join(", ", names)}))");

            return;
        }

        if (!TryConvert(column, value, out var converted))
            return;

        var paramName = NextName();
        Parameters.Add(paramName, converted);
        _fragments.Add($"({column.Expression} {OperatorSql(operatorName)} @{paramName})");
    }

    private string NextName() => "__w" + _index++;

    /// <summary>Maps a request operator onto its SQL symbol.</summary>
    public static string OperatorSql(FilterOperator operatorName) => operatorName switch
    {
        FilterOperator.NotEquals => "<>",
        FilterOperator.GreaterThan => ">",
        FilterOperator.GreaterThanOrEqual => ">=",
        FilterOperator.LessThan => "<",
        FilterOperator.LessThanOrEqual => "<=",
        _ => "=",
    };

    /// <summary>
    /// Turns a value into something Npgsql can send for a PostgreSQL column: an enum becomes its
    /// stored number, and a date without a kind is read as UTC the way every other read in this
    /// layer does.
    /// </summary>
    public static object? Normalize(object? value) => value switch
    {
        null => null,
        var v when v.GetType().IsEnum => Convert.ChangeType(v, Enum.GetUnderlyingType(v.GetType()), CultureInfo.InvariantCulture),
        DateTime { Kind: DateTimeKind.Unspecified } moment => DateTime.SpecifyKind(moment, DateTimeKind.Utc),
        _ => value
    };

    /// <summary>
    /// Converts one client value to something Npgsql can send for this column type. Anything
    /// unparsable is reported as unsupported so the caller drops the condition.
    /// </summary>
    public static bool TryConvert(SqlColumn column, string value, out object converted)
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
