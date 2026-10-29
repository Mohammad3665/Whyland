using System.Text.RegularExpressions;
using Dapper;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// A finished statement with its parameters, kept apart from execution so a repository can read
/// the SQL it is about to run (or hand it to a caller that owns the connection).
/// </summary>
/// <param name="Sql">The SQL text.</param>
/// <param name="Parameters">The values the statement binds; never inlined into the text.</param>
public sealed record DapperSqlQuery(string Sql, DynamicParameters Parameters)
{
    /// <summary>The shape of a generated parameter name; nothing else in a statement is renamed.</summary>
    private static readonly Regex GeneratedName = new(@"__w\d+", RegexOptions.CultureInvariant);

    /// <summary>
    /// Returns the statement with every generated parameter name prefixed. Several statements sent
    /// as one batch share a single parameter bag, and each numbers its own parameters from zero, so
    /// the names have to be told apart before the texts are concatenated.
    /// </summary>
    /// <param name="prefix">Text put in front of each generated name.</param>
    public DapperSqlQuery WithParameterPrefix(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        var renamed = new DynamicParameters();

        foreach (var name in Parameters.ParameterNames)
            renamed.Add(prefix + name, Parameters.Get<object?>(name));

        return this with
        {
            Sql = GeneratedName.Replace(Sql, match => prefix + match.Value),
            Parameters = renamed,
        };
    }
}
