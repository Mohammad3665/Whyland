using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Builds a Dapper statement from typed property expressions instead of hand-written SQL:
/// the FROM clause, joins, the output columns, the filters, the ORDER BY and the paging, with
/// every value bound as a parameter.
/// <para>
/// Identifiers never come from a request. They come from the property expressions and from the
/// whitelist built by <see cref="SortableFrom{TSource}"/> and <see cref="SortableExpression"/>, and
/// each one is validated and quoted before it reaches the text, so an unknown column is a
/// build-time error rather than an injected statement.
/// </para>
/// <para>
/// Column names are the CLR property names, which is how EF Core maps this PostgreSQL schema
/// (<c>"FirstName"</c>, <c>"IsDeleted"</c>), and tables are written as PostgreSQL does:
/// <c>identity.Users</c> becomes <c>"identity"."Users"</c>.
/// </para>
/// <para>
/// One object serves three read shapes: a fixed read (<see cref="Build"/>), a page driven by a
/// <see cref="QueryRequest"/> (<see cref="BuildPaged"/>) and a server-side data table driven by a
/// <see cref="DataTableFilter"/> (<see cref="ToDataTableSource"/>). All three write their WHERE
/// clause through <see cref="SqlConditionWriter"/>, which is also what a declarative
/// <see cref="SqlDataTableSpec"/> runs on, so filter semantics cannot drift between them.
/// </para>
/// </summary>
/// <typeparam name="TEntity">The entity the root table is mapped from; it owns alias <c>t0</c>.</typeparam>
public sealed class DapperFluentQueryBuilder<TEntity> : IDapperFluentQueryBuilder
{
    /// <summary>
    /// Alias of the root table, so a join can never shadow it. It is public because a computed
    /// column written with <see cref="SelectExpression"/> has to name the table it reads from.
    /// </summary>
    public const string RootAlias = "t0";

    /// <summary>PostgreSQL identifier shape; anything else is rejected before it reaches the SQL.</summary>
    private static readonly Regex IdentifierRegex = new(@"\A[A-Za-z_][A-Za-z0-9_]*\z", RegexOptions.CultureInvariant);

    /// <summary>Types a property can be read as a column; navigations and collections are not columns.</summary>
    private static readonly HashSet<Type> ScalarTypes =
    [
        typeof(string), typeof(bool), typeof(Guid), typeof(DateTime), typeof(DateTimeOffset),
        typeof(DateOnly), typeof(TimeOnly),
        typeof(byte), typeof(short), typeof(int), typeof(long),
        typeof(sbyte), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(decimal),
    ];

    private string _fromSql;
    private string _isDeletedColumnName;

    private readonly Dictionary<Type, string> _entityAliases = new()
    {
        [typeof(TEntity)] = RootAlias
    };

    private readonly HashSet<string> _aliases = new(StringComparer.Ordinal) { RootAlias };

    private readonly List<string> _selectColumns = [];
    private readonly HashSet<string> _outputNames = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<string> _joins = [];

    /// <summary>The conditions the code fixed; a request can neither add to nor remove from these.</summary>
    private readonly SqlConditionWriter _where = new();

    /// <summary>The whitelist a request may name - for sorting and for filtering alike.</summary>
    private readonly Dictionary<string, SqlColumn> _columns = new(StringComparer.OrdinalIgnoreCase);

    private string _countExpression = "COUNT(*)";
    private bool _countWasSet;
    private string? _sortKey;
    private string? _sortExpression;
    private bool _sortDescending;
    private string? _tieBreaker;
    private string? _defaultOrder;
    private int? _limit;
    private int _nextAlias = 1;
    private bool _ignoreQueryFilters;
    private bool _distinct;
    private readonly List<SqlColumn> _searchColumns = [];

    private DapperFluentQueryBuilder(string fromTable, string isDeletedColumnName, Expression<Func<TEntity, object?>>[] selectProperties)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromTable);
        ArgumentException.ThrowIfNullOrWhiteSpace(isDeletedColumnName);
        ArgumentNullException.ThrowIfNull(selectProperties);

        ValidateIdentifier(isDeletedColumnName);

        _fromSql = $"{QuoteTable(fromTable)} {QuoteIdentifier(RootAlias)}";
        _isDeletedColumnName = isDeletedColumnName;

        if (selectProperties.Length == 0)
            SelectScalarColumns();
        else
            foreach (var property in selectProperties)
                Select(property);
    }

    /// <summary>The bare form used for a spec-built query, where the SQL text comes from the spec.</summary>
    private DapperFluentQueryBuilder()
    {
        _fromSql = string.Empty;
        _isDeletedColumnName = "IsDeleted";
        _ignoreQueryFilters = true;
    }

    #region Factory

    /// <summary>
    /// Starts a builder over one table.
    /// </summary>
    /// <param name="fromTable">Table name, with its schema when the schema is not the default.</param>
    /// <param name="selectProperties">Output columns; with none, every scalar property of the entity.</param>
    public static DapperFluentQueryBuilder<TEntity> Create(
        string fromTable,
        params Expression<Func<TEntity, object?>>[] selectProperties)
        => new(fromTable, "IsDeleted", selectProperties);

    /// <summary>Starts a builder over one table with a soft-delete column other than <c>IsDeleted</c>.</summary>
    /// <param name="fromTable">Table name, with its schema when the schema is not the default.</param>
    /// <param name="isDeletedColumnName">Column holding the soft-delete flag.</param>
    /// <param name="selectProperties">Output columns; with none, every scalar property of the entity.</param>
    public static DapperFluentQueryBuilder<TEntity> Create(
        string fromTable,
        string isDeletedColumnName,
        params Expression<Func<TEntity, object?>>[] selectProperties)
        => new(fromTable, isDeletedColumnName, selectProperties);

    /// <summary>
    /// Starts a builder that adopts a declarative list definition: the FROM clause, the base
    /// predicate, the column whitelist and the fallback order all come from the spec, so a page
    /// described by a spec and a page described by builder calls run on the same engine.
    /// A spec-built query is read through <see cref="ToDataTableSource"/>; its columns are already
    /// quoted, so the typed registration methods do not apply to it.
    /// </summary>
    /// <param name="spec">The table, joins and column whitelist of the list.</param>
    public static DapperFluentQueryBuilder<TEntity> FromSpec(SqlDataTableSpec spec)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var builder = new DapperFluentQueryBuilder<TEntity>
        {
            _fromSql = spec.From,
            _defaultOrder = spec.DefaultOrder,
        };

        // The spec owns the predicate the list starts from, so it is seeded as the base instead of
        // being left for the soft-delete guard - which this builder does not apply, since the FROM
        // clause of a spec may join several tables.
        builder._where.BaseWhere = spec.BaseWhere;

        foreach (var (name, column) in spec.Columns)
            builder._columns[name] = column;

        if (spec.SearchColumns.Count > 0)
            builder._searchColumns.AddRange(spec.SearchColumns);

        return builder;
    }

    #endregion

    #region Select

    /// <summary>Adds one column of the root table to the output.</summary>
    /// <param name="column">Property of the root entity.</param>
    /// <param name="outputName">Result column name; the property name when null.</param>
    public DapperFluentQueryBuilder<TEntity> Select(Expression<Func<TEntity, object?>> column, string? outputName = null)
        => SelectFrom(column, outputName);

    /// <summary>Adds one column of the root table or of a joined table to the output.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="outputName">Result column name; the property name when null.</param>
    public DapperFluentQueryBuilder<TEntity> SelectFrom<TSource>(Expression<Func<TSource, object?>> column, string? outputName = null)
    {
        var property = GetPropertyName(column);

        return AddSelect(
            QuoteColumn(GetAlias(typeof(TSource)), property),
            outputName ?? property,
            PropertyType(typeof(TSource), property),
            clientFacing: true);
    }

    /// <summary>Adds an already-resolved <c>alias.Column</c> expression under a chosen result name.</summary>
    /// <param name="column">Qualified column, e.g. <c>t0.FirstName</c>.</param>
    /// <param name="outputName">Name the column is returned under.</param>
    public DapperFluentQueryBuilder<TEntity> SelectColumn(string column, string outputName)
        => AddSelect(QuoteColumn(column), outputName, typeof(string), clientFacing: false);

    /// <summary>
    /// Adds a computed column - an expression the code writes, such as a concatenation or a
    /// <c>CASE</c> - under the result name the row type expects. The expression is inserted as it
    /// is, so it must never be assembled from anything a client sent.
    /// </summary>
    /// <param name="expression">Raw SQL expression, e.g. <c>t0."FirstName" || ' ' || t0."LastName"</c>.</param>
    /// <param name="outputName">Name the column is returned under.</param>
    public DapperFluentQueryBuilder<TEntity> SelectExpression(string expression, string outputName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        ValidateIdentifier(outputName);

        if (!_outputNames.Add(outputName))
            throw new ArgumentException($"Duplicate output column: {outputName}", nameof(outputName));

        _selectColumns.Add($"{expression} AS {QuoteIdentifier(outputName)}");

        return this;
    }

    /// <summary>Selects every scalar property of the entity, skipping navigations and collections.</summary>
    private DapperFluentQueryBuilder<TEntity> SelectScalarColumns()
    {
        foreach (var property in typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var type = Unwrap(property.PropertyType);

            // An enum is a stored number and a column like any other: leaving it out would make a
            // full entity read return the default value for that field without saying so.
            if (!type.IsEnum && !ScalarTypes.Contains(type))
                continue;

            AddSelect(QuoteColumn(RootAlias, property.Name), property.Name, type, clientFacing: true);
        }

        return this;
    }

    /// <summary>
    /// Empties the output list. Used by a read whose columns all come from a joined table, where
    /// the columns the factory added for the root table would otherwise end up in the SELECT list.
    /// The whitelist built so far stays, since it only ever limits what a request may name.
    /// </summary>
    public DapperFluentQueryBuilder<TEntity> ClearSelect()
    {
        _selectColumns.Clear();
        _outputNames.Clear();

        return this;
    }

    /// <summary>
    /// The quoted column of the root table - <c>"t0"."FirstName"</c> - for a computed expression
    /// written with <see cref="SelectExpression"/> or <see cref="SortableExpression"/>.
    /// Only the root alias is reachable this way, and the name is validated, so it stays a
    /// code-side helper rather than a door for request text.
    /// </summary>
    /// <param name="propertyName">Property of the root entity.</param>
    public static string RootColumn(string propertyName) => QuoteColumn(RootAlias, propertyName);

    private DapperFluentQueryBuilder<TEntity> AddSelect(
        string quotedColumn,
        string outputName,
        Type clrType,
        bool clientFacing)
    {
        ValidateIdentifier(outputName);

        if (!_outputNames.Add(outputName))
            throw new ArgumentException($"Duplicate output column: {outputName}", nameof(outputName));

        _selectColumns.Add($"{quotedColumn} AS {QuoteIdentifier(outputName)}");

        // A column the row already returns can be named by a request without leaking anything new,
        // so it joins the whitelist under the same name.
        if (clientFacing)
            _columns[outputName] = new SqlColumn(quotedColumn, clrType);

        return this;
    }

    #endregion

    #region Joins

    /// <summary>Inner-joins a table to the root table.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Root-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    public DapperFluentQueryBuilder<TEntity> Join<TJoin>(string table, Expression<Func<TEntity, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn)
    {
        AddJoin(table, leftColumn, rightColumn, false, null);
        return this;
    }

    /// <summary>Inner-joins a table to the root table under a chosen alias.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Root-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    /// <param name="alias">SQL alias, so the same table can be joined more than once.</param>
    public TableRef<TJoin> Join<TJoin>(string table, Expression<Func<TEntity, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn, string alias)
        => new(this, AddJoin(table, leftColumn, rightColumn, false, alias));

    /// <summary>Left-joins a table so a missing related row keeps the root row.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Root-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    public DapperFluentQueryBuilder<TEntity> LeftJoin<TJoin>(string table, Expression<Func<TEntity, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn)
    {
        AddJoin(table, leftColumn, rightColumn, true, null);
        return this;
    }

    /// <summary>Left-joins a table to the root table under a chosen alias.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Root-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    /// <param name="alias">SQL alias, so the same table can be joined more than once.</param>
    public TableRef<TJoin> LeftJoin<TJoin>(string table, Expression<Func<TEntity, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn, string alias)
        => new(this, AddJoin(table, leftColumn, rightColumn, true, alias));

    /// <summary>Inner-joins a table to a previously joined table.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Left-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    public DapperFluentQueryBuilder<TEntity> Join<TLeft, TJoin>(string table, Expression<Func<TLeft, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn)
    {
        AddJoin(table, leftColumn, rightColumn, false, null);
        return this;
    }

    /// <summary>Inner-joins a table to a previously joined table under a chosen alias.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Left-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    /// <param name="alias">SQL alias, so the same table can be joined more than once.</param>
    public TableRef<TJoin> Join<TLeft, TJoin>(string table, Expression<Func<TLeft, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn, string alias)
        => new(this, AddJoin(table, leftColumn, rightColumn, false, alias));

    /// <summary>Left-joins a table to a previously joined table.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Left-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    public DapperFluentQueryBuilder<TEntity> LeftJoin<TLeft, TJoin>(string table, Expression<Func<TLeft, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn)
    {
        AddJoin(table, leftColumn, rightColumn, true, null);
        return this;
    }

    /// <summary>Left-joins a table to a previously joined table under a chosen alias.</summary>
    /// <param name="table">Table to add.</param>
    /// <param name="leftColumn">Left-side join property.</param>
    /// <param name="rightColumn">New-table join property.</param>
    /// <param name="alias">SQL alias, so the same table can be joined more than once.</param>
    public TableRef<TJoin> LeftJoin<TLeft, TJoin>(string table, Expression<Func<TLeft, object?>> leftColumn, Expression<Func<TJoin, object?>> rightColumn, string alias)
        => new(this, AddJoin(table, leftColumn, rightColumn, true, alias));

    private string AddJoin(string table, LambdaExpression leftColumn, LambdaExpression rightColumn, bool isLeftJoin, string? alias)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        var leftAlias = GetAlias(leftColumn.Parameters[0].Type);
        var joinAlias = alias is null ? NextAutoAlias() : UseAlias(alias);

        var leftSql = $"{QuoteIdentifier(leftAlias)}.{QuoteIdentifier(GetPropertyName(leftColumn))}";
        var rightSql = $"{QuoteIdentifier(joinAlias)}.{QuoteIdentifier(GetPropertyName(rightColumn))}";

        _joins.Add($"{(isLeftJoin ? "LEFT JOIN" : "INNER JOIN")} {QuoteTable(table)} {QuoteIdentifier(joinAlias)} ON {leftSql} = {rightSql}");

        // An explicitly aliased join never takes over the alias its type already owns, so the typed
        // helpers keep pointing at the first one and the handle points at this one.
        _entityAliases.TryAdd(rightColumn.Parameters[0].Type, joinAlias);

        return joinAlias;
    }

    private string NextAutoAlias()
    {
        var alias = $"t{_nextAlias++}";

        if (!_aliases.Add(alias))
            throw new InvalidOperationException($"Alias '{alias}' is already used.");

        return alias;
    }

    private string UseAlias(string alias)
    {
        ValidateIdentifier(alias);

        if (!_aliases.Add(alias))
            throw new InvalidOperationException($"Alias '{alias}' is already used.");

        return alias;
    }

    /// <summary>
    /// Columns of one joined table, reached through the handle its join returns. It exists because
    /// the same table can be joined twice - the creator and the editor of a row are both read from
    /// the users table - and a type-keyed lookup cannot tell them apart.
    /// </summary>
    /// <typeparam name="TOther">The entity the aliased table is mapped from.</typeparam>
    public sealed class TableRef<TOther>
    {
        private readonly DapperFluentQueryBuilder<TEntity> _owner;
        private readonly string _alias;

        internal TableRef(DapperFluentQueryBuilder<TEntity> owner, string alias)
        {
            _owner = owner;
            _alias = alias;
        }

        /// <summary>The builder, so the chain can continue after the aliased columns are declared.</summary>
        public DapperFluentQueryBuilder<TEntity> Builder => _owner;

        /// <summary>Adds one column of this table to the output.</summary>
        /// <param name="column">Property of this table.</param>
        /// <param name="outputName">Result column name; the property name when null.</param>
        public TableRef<TOther> Select(Expression<Func<TOther, object?>> column, string? outputName = null)
        {
            var property = GetPropertyName(column);

            _owner.AddSelect(
                QuoteColumn(_alias, property),
                outputName ?? property,
                PropertyType(typeof(TOther), property),
                clientFacing: true);

            return this;
        }

        /// <summary>Adds an equality condition on a column of this table.</summary>
        public TableRef<TOther> Where(Expression<Func<TOther, object?>> column, object? value)
            => Where(column, SqlComparisonType.Equal, value);

        /// <summary>Adds a comparison of any kind on a column of this table.</summary>
        /// <param name="column">Property of this table.</param>
        /// <param name="comparison">How the bound value is compared.</param>
        /// <param name="value">Value to bind; null becomes an IS [NOT] NULL test.</param>
        public TableRef<TOther> Where(Expression<Func<TOther, object?>> column, SqlComparisonType comparison, object? value)
        {
            _owner.AddWhere(QuoteColumn(_alias, GetPropertyName(column)), comparison, value);
            return this;
        }

        /// <summary>Registers one column of this table so a request may sort and filter by it.</summary>
        /// <param name="column">Property of this table.</param>
        /// <param name="key">Name a request uses; the property name when null.</param>
        public TableRef<TOther> Sortable(Expression<Func<TOther, object?>> column, string? key = null)
        {
            var property = GetPropertyName(column);
            _owner._columns[key ?? property] = new SqlColumn(
                QuoteColumn(_alias, property), PropertyType(typeof(TOther), property));

            return this;
        }

        /// <summary>Counts distinct values of this table's column instead of rows.</summary>
        public TableRef<TOther> CountDistinct(Expression<Func<TOther, object?>> column)
        {
            _owner._countExpression = $"COUNT(DISTINCT {QuoteColumn(_alias, GetPropertyName(column))})";
            _owner._countWasSet = true;

            return this;
        }

        /// <summary>Uses this table's column as the stable secondary sort.</summary>
        public TableRef<TOther> TieBreakBy(Expression<Func<TOther, object?>> column)
        {
            _owner._tieBreaker = QuoteColumn(_alias, GetPropertyName(column));
            return this;
        }
    }

    #endregion

    #region Filters

    /// <summary>Adds an equality condition on a root-table column.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="value">Value to bind.</param>
    public DapperFluentQueryBuilder<TEntity> Where(Expression<Func<TEntity, object?>> column, object? value)
        => Where(column, SqlComparisonType.Equal, value);

    /// <summary>Adds a comparison of any kind on a root-table column.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="comparison">How the bound value is compared.</param>
    /// <param name="value">Value to bind; null becomes an IS [NOT] NULL test.</param>
    public DapperFluentQueryBuilder<TEntity> Where(Expression<Func<TEntity, object?>> column, SqlComparisonType comparison, object? value)
    {
        AddWhere(QuoteColumn(GetAlias(typeof(TEntity)), GetPropertyName(column)), comparison, value);
        return this;
    }

    /// <summary>Adds an equality condition on a column of any registered table.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="value">Value to bind.</param>
    public DapperFluentQueryBuilder<TEntity> WhereFrom<TSource>(Expression<Func<TSource, object?>> column, object? value)
        => WhereFrom(column, SqlComparisonType.Equal, value);

    /// <summary>Adds a comparison of any kind on a column of any registered table.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="comparison">How the bound value is compared.</param>
    /// <param name="value">Value to bind; null becomes an IS [NOT] NULL test.</param>
    public DapperFluentQueryBuilder<TEntity> WhereFrom<TSource>(Expression<Func<TSource, object?>> column, SqlComparisonType comparison, object? value)
    {
        AddWhere(QuoteColumn(GetAlias(typeof(TSource)), GetPropertyName(column)), comparison, value);
        return this;
    }

    /// <summary>Adds an equality condition only when the caller's own condition holds.</summary>
    /// <param name="condition">Whether the condition is added at all.</param>
    /// <param name="column">Root property.</param>
    /// <param name="value">Value to bind.</param>
    public DapperFluentQueryBuilder<TEntity> WhereIf(bool condition, Expression<Func<TEntity, object?>> column, object? value)
        => condition ? Where(column, value) : this;

    /// <summary>Adds an equality condition on any registered table only when the caller's condition holds.</summary>
    /// <param name="condition">Whether the condition is added at all.</param>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="value">Value to bind.</param>
    public DapperFluentQueryBuilder<TEntity> WhereIfFrom<TSource>(bool condition, Expression<Func<TSource, object?>> column, object? value)
        => condition ? WhereFrom(column, value) : this;

    /// <summary>Adds a set-membership test on a root-table column.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="values">Values the column must equal one of; an empty set appends nothing.</param>
    public DapperFluentQueryBuilder<TEntity> WhereIn(Expression<Func<TEntity, object?>> column, IEnumerable<object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var items = values.Where(x => x is not null).ToList();

        return items.Count == 0
            ? this
            : AddIn(QuoteColumn(GetAlias(typeof(TEntity)), GetPropertyName(column)), items);
    }

    /// <summary>Adds a set-membership test on a column of any registered table.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="values">Values the column must equal one of; an empty set appends nothing.</param>
    public DapperFluentQueryBuilder<TEntity> WhereInFrom<TSource>(Expression<Func<TSource, object?>> column, IEnumerable<object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var items = values.Where(x => x is not null).ToList();

        return items.Count == 0
            ? this
            : AddIn(QuoteColumn(GetAlias(typeof(TSource)), GetPropertyName(column)), items);
    }

    /// <summary>Adds a lower and an upper bound to the same root-table column.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="from">Inclusive lower bound; null appends nothing.</param>
    /// <param name="to">Inclusive upper bound; null appends nothing.</param>
    public DapperFluentQueryBuilder<TEntity> WhereBetween(Expression<Func<TEntity, object?>> column, object? from, object? to)
    {
        var quoted = QuoteColumn(GetAlias(typeof(TEntity)), GetPropertyName(column));

        if (from is not null) _where.AddComparison(quoted, ">=", from);
        if (to is not null) _where.AddComparison(quoted, "<=", to);

        return this;
    }

    /// <summary>
    /// Applies one condition written the way a request writes it - a whitelisted name, an operator
    /// and a text value - which is how a page reuses the field pickers of a data table.
    /// </summary>
    /// <param name="name">A name registered with <see cref="SortableFrom{TSource}"/>.</param>
    /// <param name="operatorName">How the value is compared.</param>
    /// <param name="value">The text sent by the client.</param>
    public DapperFluentQueryBuilder<TEntity> WhereColumn(string name, FilterOperator operatorName, string? value)
    {
        _where.Apply(Find(name), operatorName, value);
        return this;
    }

    /// <summary>Searches a text across several root-table columns.</summary>
    /// <param name="value">Text to look for; empty or null adds nothing.</param>
    /// <param name="columns">Columns the text is matched against, ORed.</param>
    public DapperFluentQueryBuilder<TEntity> Search(string? value, params Expression<Func<TEntity, object?>>[] columns)
        => SearchFrom(value, columns);

    /// <summary>Searches a text across several columns of any registered table, case-insensitively.</summary>
    /// <param name="value">Text to look for; empty or null adds nothing.</param>
    /// <param name="columns">Columns the text is matched against, ORed.</param>
    public DapperFluentQueryBuilder<TEntity> SearchFrom<TSource>(string? value, params Expression<Func<TSource, object?>>[] columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        if (string.IsNullOrWhiteSpace(value))
            return this;

        if (columns.Length == 0)
            throw new ArgumentException("At least one search column is required.", nameof(columns));

        var alias = GetAlias(typeof(TSource));

        return AddSearch(value, [.. columns.Select(x =>
        {
            var property = GetPropertyName(x);
            return new SqlColumn(QuoteColumn(alias, property), PropertyType(typeof(TSource), property));
        })]);
    }

    /// <summary>Searches a text across several whitelisted columns by name, case-insensitively.</summary>
    /// <param name="value">Text to look for; empty or null adds nothing.</param>
    /// <param name="names">Whitelisted column names the text is matched against, ORed.</param>
    public DapperFluentQueryBuilder<TEntity> SearchColumns(string? value, params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        if (string.IsNullOrWhiteSpace(value))
            return this;

        if (names.Length == 0)
            throw new ArgumentException("At least one search column is required.", nameof(names));

        return AddSearch(value, [.. names.Select(Required)]);
    }

    /// <summary>ORs one bound pattern over the given columns; a non-text column is skipped.</summary>
    private DapperFluentQueryBuilder<TEntity> AddSearch(string value, List<SqlColumn> columns)
    {
        _where.ApplyTextAcross(columns, FilterOperator.Contains, value.Trim());
        return this;
    }

    private DapperFluentQueryBuilder<TEntity> AddIn(string quotedColumn, List<object?> items)
    {
        _where.AddIn(quotedColumn, items);
        return this;
    }

    private void AddWhere(string quotedColumn, SqlComparisonType comparison, object? value)
    {
        if (value is null)
        {
            var negate = comparison switch
            {
                SqlComparisonType.Equal => false,
                SqlComparisonType.NotEqual => true,
                _ => throw new ArgumentException("Null is only supported with Equal and NotEqual.", nameof(value))
            };

            _where.AddNullTest(quotedColumn, negate);
            return;
        }

        _where.AddComparison(quotedColumn, comparison switch
        {
            SqlComparisonType.Equal => "=",
            SqlComparisonType.NotEqual => "<>",
            SqlComparisonType.GreaterThan => ">",
            SqlComparisonType.GreaterThanOrEqual => ">=",
            SqlComparisonType.LessThan => "<",
            SqlComparisonType.LessThanOrEqual => "<=",
            _ => throw new ArgumentOutOfRangeException(nameof(comparison))
        }, value);
    }

    #endregion

    #region Sorting

    /// <summary>Registers a root-table column so a request may sort and filter by it.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="key">Name a request uses; the property name when null.</param>
    public DapperFluentQueryBuilder<TEntity> Sortable(Expression<Func<TEntity, object?>> column, string? key = null)
        => SortableFrom(column, key);

    /// <summary>Registers a column of any registered table so a request may sort and filter by it.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    /// <param name="key">Name a request uses; the property name when null.</param>
    public DapperFluentQueryBuilder<TEntity> SortableFrom<TSource>(Expression<Func<TSource, object?>> column, string? key = null)
    {
        var property = GetPropertyName(column);
        _columns[key ?? property] = new SqlColumn(
            QuoteColumn(GetAlias(typeof(TSource)), property), PropertyType(typeof(TSource), property));

        return this;
    }

    /// <summary>
    /// Registers a computed column - one written by <see cref="SelectExpression"/> or a quoted
    /// output alias - so a request can sort and filter by it too. The expression is inserted as it
    /// is, so it must never be assembled from anything a client sent.
    /// </summary>
    /// <param name="expression">Raw SQL expression, or a quoted output alias such as <c>"Name"</c>.</param>
    /// <param name="key">Name a request uses.</param>
    /// <param name="clrType">Type the expression yields, needed when the column is filtered.</param>
    public DapperFluentQueryBuilder<TEntity> SortableExpression(string expression, string key, Type? clrType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        ValidateIdentifier(key);

        _columns[key] = new SqlColumn(expression, clrType ?? typeof(string));

        return this;
    }

    /// <summary>Picks the fixed ORDER BY from a root-table column.</summary>
    /// <param name="column">Root property.</param>
    /// <param name="descending">Descending when true.</param>
    public DapperFluentQueryBuilder<TEntity> OrderBy(Expression<Func<TEntity, object?>> column, bool descending = false)
    {
        _sortExpression = QuoteColumn(RootAlias, GetPropertyName(column));
        _sortKey = null;
        _sortDescending = descending;

        return this;
    }

    /// <summary>Picks the fixed ORDER BY from a whitelisted column.</summary>
    /// <param name="key">A name registered with <see cref="SortableFrom{TSource}"/>.</param>
    /// <param name="descending">Descending when true.</param>
    public DapperFluentQueryBuilder<TEntity> OrderBy(string key, bool descending = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _sortKey = key.Trim();
        _sortExpression = null;
        _sortDescending = descending;

        return this;
    }

    /// <summary>Adds a stable secondary sort key so two pages never repeat or skip a row.</summary>
    /// <param name="column">Root property.</param>
    public DapperFluentQueryBuilder<TEntity> TieBreakBy(Expression<Func<TEntity, object?>> column)
        => TieBreakByFrom(column);

    /// <summary>Adds a stable secondary sort key from any registered table.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    public DapperFluentQueryBuilder<TEntity> TieBreakByFrom<TSource>(Expression<Func<TSource, object?>> column)
    {
        _tieBreaker = QuoteColumn(GetAlias(typeof(TSource)), GetPropertyName(column));
        return this;
    }

    /// <summary>Overrides the ORDER BY a page falls back to when the client picks no column.</summary>
    /// <param name="orderBy">Raw ORDER BY body, e.g. <c>"t0"."CreatedAt" DESC</c>.</param>
    public DapperFluentQueryBuilder<TEntity> DefaultOrder(string orderBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderBy);
        _defaultOrder = orderBy;

        return this;
    }

    #endregion

    #region Shape

    /// <summary>Returns each row once; the page counts mirror it by counting the same set.</summary>
    public DapperFluentQueryBuilder<TEntity> Distinct()
    {
        _distinct = true;
        return this;
    }

    /// <summary>Caps the plain statement at a fixed number of rows.</summary>
    /// <param name="rows">Number of rows the statement may read.</param>
    public DapperFluentQueryBuilder<TEntity> Limit(int rows)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rows, 1);
        _limit = rows;

        return this;
    }

    /// <summary>Counts distinct values of a root column instead of rows, for one-to-many joins.</summary>
    /// <param name="column">Root property.</param>
    public DapperFluentQueryBuilder<TEntity> CountDistinct(Expression<Func<TEntity, object?>> column)
        => CountDistinctFrom(column);

    /// <summary>Counts distinct values of any registered column instead of rows.</summary>
    /// <param name="column">Property of the table the expression is typed to.</param>
    public DapperFluentQueryBuilder<TEntity> CountDistinctFrom<TSource>(Expression<Func<TSource, object?>> column)
    {
        _countExpression = $"COUNT(DISTINCT {QuoteColumn(GetAlias(typeof(TSource)), GetPropertyName(column))})";
        _countWasSet = true;

        return this;
    }

    /// <summary>Drops the automatic soft-delete predicate for this statement only.</summary>
    public DapperFluentQueryBuilder<TEntity> IgnoreQueryFilters()
    {
        _ignoreQueryFilters = true;
        return this;
    }

    #endregion

    #region Build

    /// <summary>Largest page the builder will hand out, so a made-up take cannot read the table.</summary>
    public const int MaxPageSize = SqlPaging.MaxPageSize;

    /// <summary>Builds the finished statement.</summary>
    public DapperSqlQuery Build()
    {
        var where = Compose();
        var order = BuildOrderBy(_sortKey, _sortDescending);

        var sql = $"""
            SELECT
                {(_distinct ? "DISTINCT " : string.Empty)}{RequireSelectList()}
            FROM {_fromSql}
            {BuildJoins()}
            {(where.Sql.Length == 0 ? string.Empty : "WHERE " + where.Sql)}
            {order}
            {BuildLimit()};
            """;

        return new DapperSqlQuery(sql, where.Parameters);
    }

    /// <summary>
    /// Builds the count statement and the page statement as one batch. The request's own field
    /// conditions are applied through the shared engine, so a page sent as a
    /// <see cref="QueryRequest"/> filters exactly like a data table does.
    /// Grouping is not part of this builder, so a request that carries it reads ungrouped rows.
    /// </summary>
    /// <param name="request">Page, page size, requested sort and field conditions.</param>
    /// <param name="defaultSort">Sort key used when the request names none, or names one that is not registered.</param>
    public PagedSqlQuery BuildPaged(QueryRequest request, string defaultSort = "Id")
    {
        ArgumentNullException.ThrowIfNull(request);

        var where = Compose();

        foreach (var condition in request.Filters ?? [])
            where.Apply(Find(condition.Property), condition.Operator, condition.Value);

        var (take, skip) = SqlPaging.From(request);
        where.Parameters.Add("__limit", take);
        where.Parameters.Add("__offset", skip);

        var sortKey = ResolveSortKey(request.SortBy, defaultSort);

        var joins = BuildJoins();
        var whereSql = where.Sql;
        var selectList = RequireSelectList();

        var sql = $"""
            {BuildCount(whereSql, selectList)};

            SELECT
                {(_distinct ? "DISTINCT " : string.Empty)}{selectList}
            FROM {_fromSql}
            {joins}
            {(whereSql.Length == 0 ? string.Empty : "WHERE " + whereSql)}
            {BuildOrderBy(sortKey, request.Sort == SortType.Desc)}
            LIMIT @__limit
            OFFSET @__offset;
            """;

        return new PagedSqlQuery(sql, where.Parameters, request.Page < 1 ? 1 : request.Page, take);
    }

    /// <summary>
    /// Hands the builder to the data-table engine: the FROM clause, the fixed conditions, the
    /// whitelist the registrations built and the fallback order all come from here.
    /// </summary>
    public SqlDataTableSource ToDataTableSource()
        => new(_fromSql, Compose(), _columns,
            _defaultOrder ?? BuildOrderByBody(_sortKey, _sortDescending), _distinct, _searchColumns);

    /// <summary>The base predicate plus every condition the code fixed, as one writer.</summary>
    private SqlConditionWriter Compose()
    {
        var where = _where.Clone();

        // A spec-built writer already carries the predicate the list starts from; a builder that
        // turned query filters off must not replace it with the soft-delete guard.
        if (!_ignoreQueryFilters && string.IsNullOrWhiteSpace(where.BaseWhere))
            where.BaseWhere = $"{QuoteColumn(RootAlias, _isDeletedColumnName)} = FALSE";

        return where;
    }

    /// <summary>The count statement, de-duplicated the same way the rows are.</summary>
    private string BuildCount(string whereSql, string selectList)
    {
        var where = whereSql.Length == 0 ? "TRUE" : whereSql;

        if (_countWasSet)
            return $"SELECT {_countExpression} FROM {_fromSql}{BuildJoins()} WHERE {where}";

        return _distinct
            ? $"SELECT COUNT(*) FROM (SELECT DISTINCT {selectList} FROM {_fromSql}{BuildJoins()} WHERE {where}) __distinct_rows"
            : $"SELECT COUNT(*) FROM {_fromSql}{BuildJoins()} WHERE {where}";
    }

    private string RequireSelectList()
        => _selectColumns.Count == 0
            ? throw new InvalidOperationException(
                $"The statement over '{_fromSql}' has no output columns.")
            : string.Join("," + Environment.NewLine, _selectColumns);

    private string ResolveSortKey(string? requested, string defaultSort)
    {
        var candidate = (requested ?? string.Empty).Trim();

        if (candidate.Length > 0 && _columns.ContainsKey(candidate))
            return candidate;

        var fallback = defaultSort.Trim();

        if (!_columns.ContainsKey(fallback))
            throw new InvalidOperationException(
                $"Default sort field '{defaultSort}' is not registered with Sortable(...). "
                + $"Registered keys: {string.Join(", ", _columns.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))}.");

        return fallback;
    }

    #endregion

    #region Private helpers

    private SqlColumn? Find(string? name)
        => string.IsNullOrWhiteSpace(name)
            ? null
            : _columns.TryGetValue(name, out var column) ? column : null;

    private SqlColumn Required(string name)
        => _columns.TryGetValue(name, out var column)
            ? column
            : throw new InvalidOperationException(
                $"Column '{name}' is not registered. Register it with Sortable(...) first.");

    private string BuildLimit() => _limit is null ? string.Empty : $"LIMIT {_limit.Value}";

    private string BuildJoins()
        => _joins.Count == 0 ? string.Empty : string.Join(Environment.NewLine, _joins);

    private string BuildOrderBy(string? key, bool descending)
    {
        var body = BuildOrderByBody(key, descending);
        return body.Length == 0 ? string.Empty : "ORDER BY " + body;
    }

    /// <summary>
    /// The ORDER BY body: the requested key - or the fixed expression, or the fallback order -
    /// then the tiebreaker when it differs, so two pages never show the same row twice.
    /// </summary>
    private string BuildOrderByBody(string? key, bool descending)
    {
        string? column;

        if (key is null)
        {
            if (_sortExpression is not null) return AppendTieBreaker(_sortExpression, descending);
            return _defaultOrder ?? string.Empty;
        }

        if (!_columns.TryGetValue(key.Trim(), out var entry))
            throw new ArgumentException($"Unsupported sort field: {key}", nameof(key));

        column = entry.Expression;

        return AppendTieBreaker(column, descending);
    }

    private string AppendTieBreaker(string column, bool descending)
    {
        var orderBy = $"{column} {(descending ? "DESC" : "ASC")}";

        if (_tieBreaker is not null && !string.Equals(column, _tieBreaker, StringComparison.OrdinalIgnoreCase))
            orderBy += $", {_tieBreaker} ASC";

        return orderBy;
    }

    private string GetAlias(Type entityType)
        => !_entityAliases.TryGetValue(entityType, out var alias)
            ? throw new InvalidOperationException($"Entity '{entityType.Name}' has not been joined.")
            : alias;

    private static Type PropertyType(Type entityType, string property)
        => entityType.GetProperty(property, BindingFlags.Public | BindingFlags.Instance) is { } info
            ? Unwrap(info.PropertyType)
            : throw new ArgumentException($"Type '{entityType.Name}' has no property '{property}'.", nameof(property));

    private static string QuoteColumn(string alias, string name)
    {
        ValidateIdentifier(alias);
        ValidateIdentifier(name);

        return $"{QuoteIdentifier(alias)}.{QuoteIdentifier(name)}";
    }

    private string QuoteColumn(string column)
    {
        var parts = column.Split('.');

        if (parts.Length != 2)
            throw new ArgumentException("Column must use alias.Column format.", nameof(column));

        var alias = parts[0].Trim();

        return !_aliases.Contains(alias)
            ? throw new ArgumentException($"Unknown table alias: {alias}", nameof(column))
            : QuoteColumn(alias, parts[1]);
    }

    private static string GetPropertyName(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Expression body = expression.Body;

        while (body is UnaryExpression unary &&
               (unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked))
        {
            body = unary.Operand;
        }

        if (body is not MemberExpression { Member: PropertyInfo } member || member.Expression != expression.Parameters[0])
            throw new ArgumentException("Expression must select a direct property.", nameof(expression));

        return member.Member.Name;
    }

    private static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    private static string QuoteTable(string table)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        var parts = table.Split('.');

        return parts.Length is < 1 or > 2
            ? throw new ArgumentException("Table must use table or schema.table format.", nameof(table))
            : string.Join(".", parts.Select(QuoteIdentifier));
    }

    private static string QuoteIdentifier(string value)
    {
        ValidateIdentifier(value);
        return $"\"{value}\"";
    }

    private static void ValidateIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 63 || !IdentifierRegex.IsMatch(value))
            throw new ArgumentException($"Invalid SQL identifier: {value}", nameof(value));
    }

    #endregion
}
