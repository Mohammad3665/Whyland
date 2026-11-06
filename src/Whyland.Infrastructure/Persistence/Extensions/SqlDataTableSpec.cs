namespace Whyland.Infrastructure.Persistence.Extensions;

/// <summary>
/// One column a client may select, filter or sort by: the SQL expression it is read from
/// and the CLR type its values are converted to.
/// <para>
/// A membership is the exception: it names a relationship instead of a column — the roles an
/// account holds — so it carries a predicate template and is filtered but never shown or ordered.
/// </para>
/// </summary>
/// <param name="Expression">The qualified SQL expression, e.g. <c>u."FirstName"</c>.</param>
/// <param name="ClrType">The type of the underlying column, after unwrapping nullability.</param>
/// <param name="Membership">
/// For a relationship: an SQL template with one <c>{value}</c> hole that the filter fills with a
/// parameter, e.g. an <c>EXISTS</c> over an assignment table. Null for an ordinary column.
/// </param>
public sealed record SqlColumn(string Expression, Type ClrType, string? Membership = null)
{
    /// <summary>Whether this entry filters through a relationship rather than reading a column.</summary>
    public bool IsMembership => Membership is not null;
}

/// <summary>
/// The Dapper counterpart of the generic <c>QueryRequest</c> pipeline: everything a
/// server-side data-table read needs, expressed as SQL instead of expression trees.
/// <para>
/// <see cref="Columns"/> is the whitelist — identifiers are taken from it and never from the
/// request, and every value is sent as a parameter, so a crafted field name cannot inject SQL.
/// The same spec also drives the SELECT list, the ORDER BY and the filters, which is why a new
/// column costs one dictionary entry instead of one query.
/// </para>
/// </summary>
public sealed class SqlDataTableSpec
{
    /// <summary>Types a single value can filter; anything else is a navigation or a collection.</summary>
    private static readonly HashSet<Type> ScalarTypes =
    [
        typeof(string), typeof(bool), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset),
        typeof(DateOnly), typeof(TimeOnly),
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];

    /// <summary>
    /// Creates a spec.
    /// </summary>
    /// <param name="from">The FROM clause body: the table plus its alias and any join.</param>
    /// <param name="baseWhere">The predicate that always applies, e.g. the not-deleted filter.</param>
    /// <param name="defaultOrder">The ORDER BY used when the client did not pick a column.</param>
    /// <param name="columns">The whitelist of usable columns, keyed by property name.</param>
    /// <param name="searchColumns">
    /// Columns a free-text search is matched against when the request names no field. With none,
    /// the first text column of the whitelist is used.
    /// </param>
    public SqlDataTableSpec(
        string from,
        string baseWhere,
        string defaultOrder,
        IReadOnlyDictionary<string, SqlColumn> columns,
        IEnumerable<string>? searchColumns = null)
    {
        From = from;
        BaseWhere = baseWhere;
        DefaultOrder = defaultOrder;
        Columns = columns;

        var group = new List<SqlColumn>();
        foreach (var name in searchColumns ?? [])
            if (columns.TryGetValue(name, out var column)) group.Add(column);

        SearchColumns = group;
    }

    /// <summary>The table and its joins, without the FROM keyword.</summary>
    public string From { get; }

    /// <summary>The predicate every query starts from.</summary>
    public string BaseWhere { get; }

    /// <summary>The fallback ORDER BY.</summary>
    public string DefaultOrder { get; }

    /// <summary>The usable columns, matched case-insensitively (clients send camelCase).</summary>
    public IReadOnlyDictionary<string, SqlColumn> Columns { get; }

    /// <summary>The ORed group a fieldless free-text search reads.</summary>
    public IReadOnlyList<SqlColumn> SearchColumns { get; }

    /// <summary>Resolves a client-supplied column name, or null when it is not whitelisted.</summary>
    /// <param name="name">The property name coming from the request.</param>
    public SqlColumn? Find(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? null
            : Columns.TryGetValue(name, out var column) ? column : null;

    /// <summary>First text column, used when the client searches without naming a field.</summary>
    public SqlColumn? DefaultSearchColumn()
        => Columns.Values.FirstOrDefault(x => x.ClrType == typeof(string));

    /// <summary>
    /// Builds the column map of an entity: every scalar property becomes
    /// <c>alias."PropertyName"</c>, which is the column EF Core maps it to. An identifier property
    /// is registered under the raw key type it wraps, since that is the type the column stores.
    /// Navigations and
    /// collections are skipped because no single value can filter them.
    /// </summary>
    /// <typeparam name="TEntity">The aggregate the table is read from.</typeparam>
    /// <param name="alias">The SQL alias of that table.</param>
    /// <param name="exclude">
    /// Properties kept out of the whitelist. Anything the client must never read, search or sort
    /// by (a password hash, for example) has to be listed here, because a whitelisted column is
    /// reachable by name from a request.
    /// </param>
    public static Dictionary<string, SqlColumn> ColumnsOf<TEntity>(
        string alias,
        IEnumerable<string>? exclude = null)
    {
        var excluded = new HashSet<string>(exclude ?? [], StringComparer.OrdinalIgnoreCase);
        var columns = new Dictionary<string, SqlColumn>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // A wrapped key is stored as the type it wraps, so the whitelist describes the column
            // by that raw type. Left as the identifier itself, it would look like an unknown
            // struct, drop out of the whitelist silently, and stop being filterable.
            var type = StronglyTypedIds.Unwrap(property.PropertyType);

            if (excluded.Contains(property.Name))
                continue;

            if (type.IsEnum || ScalarTypes.Contains(type))
                columns[property.Name] = new SqlColumn($"{alias}.\"{property.Name}\"", type);
        }

        return columns;
    }

    /// <summary>
    /// Returns a copy of <paramref name="columns"/> with joined or computed columns added, e.g.
    /// the avatar of a profile table the row is joined to, and with the relationships the list can
    /// be filtered by added as memberships, e.g. the roles an account holds.
    /// </summary>
    /// <param name="columns">The entity columns to extend.</param>
    /// <param name="extras">Name, SQL expression and CLR type of each added column.</param>
    /// <param name="memberships">
    /// Name, predicate template and CLR type of each relationship. The template carries the
    /// <c>{value}</c> hole its filter binds, so a relationship is whitelisted like any other
    /// field and still never reaches the statement as text.
    /// </param>
    public static IReadOnlyDictionary<string, SqlColumn> WithExtras(
        Dictionary<string, SqlColumn> columns,
        IEnumerable<(string Name, string Expression, Type ClrType)> extras,
        IEnumerable<(string Name, string Template, Type ClrType)>? memberships = null)
    {
        var extended = new Dictionary<string, SqlColumn>(columns, StringComparer.OrdinalIgnoreCase);

        foreach (var (name, expression, clrType) in extras)
            extended[name] = new SqlColumn(expression, clrType);

        foreach (var (name, template, clrType) in memberships ?? [])
            extended[name] = new SqlColumn(template, clrType, template);

        return extended;
    }

    #region Audit authors

    /// <summary>The alias the creating account is joined under.</summary>
    public const string CreatedByAlias = "cu";

    /// <summary>The alias the last editing account is joined under.</summary>
    public const string UpdatedByAlias = "uu";

    /// <summary>
    /// One join that reads an audit author by its key. A LEFT JOIN over a primary key: an account is
    /// one row, so the audited table keeps exactly one row too, and a row the seeder made — which has
    /// no author — stays in the list with no name instead of vanishing from it.
    /// </summary>
    /// <param name="table">Alias of the audited table.</param>
    /// <param name="alias">Alias to join the account under, <see cref="CreatedByAlias"/> or <see cref="UpdatedByAlias"/>.</param>
    /// <param name="column">Audit column of the audited table: <c>CreatedBy</c> or <c>UpdatedBy</c>.</param>
    public static string AuthorJoin(string table, string alias, string column)
        => $"LEFT JOIN {SchemaConstants.Identity}.\"Users\" {alias} ON {alias}.\"Id\" = {table}.\"{column}\"";

    /// <summary>
    /// The two whitelist entries of <see cref="AuthorJoin"/>. An admin reads a name, never the guid an
    /// audit column stores, and the spelling of a name is decided here so that no page writes its own
    /// concatenation and the details modals cannot drift apart.
    /// </summary>
    public static (string Name, string Expression, Type ClrType)[] AuthorNames() =>
    [
        ("CreatedByName", FullName(CreatedByAlias), typeof(string)),
        ("UpdatedByName", FullName(UpdatedByAlias), typeof(string)),
    ];

    /// <summary>How an account is written out: given name, space, family name.</summary>
    private static string FullName(string alias)
        => $"{alias}.\"FirstName\" || ' ' || {alias}.\"LastName\"";

    #endregion
}
