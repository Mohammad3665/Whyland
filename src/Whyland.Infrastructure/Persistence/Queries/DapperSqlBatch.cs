using System.Text;

namespace Whyland.Infrastructure.Persistence.Queries;

/// <summary>
/// Runs several built statements as one database round-trip and hands the caller the multi-reader
/// while the connection is still open. A screen that shows one row of an aggregate next to the rows
/// under it (an account with its roles and its provider links) is one batch, not three hops.
/// </summary>
public static class DapperSqlBatch
{
    /// <summary>
    /// Sends the statements in order and maps the readers in <paramref name="read"/>. The callback
    /// runs before the connection closes, which is the only moment a grid reader is usable.
    /// </summary>
    /// <typeparam name="TResult">What the caller builds out of the readers.</typeparam>
    /// <param name="dataSource">The source the connection is opened from.</param>
    /// <param name="read">Maps the readers, in the order the statements were passed.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <param name="builders">The statements, each already carrying its own fixed conditions.</param>
    public static async Task<TResult> RunAsync<TResult>(
        NpgsqlDataSource dataSource,
        Func<SqlMapper.GridReader, TResult> read,
        CancellationToken ct,
        params IDapperFluentQueryBuilder[] builders)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(read);
        ArgumentNullException.ThrowIfNull(builders);

        if (builders.Length == 0)
            throw new ArgumentException("A batch needs at least one statement.", nameof(builders));

        var sql = new StringBuilder();
        var parameters = new DynamicParameters();

        for (var index = 0; index < builders.Length; index++)
        {
            var query = builders[index].Build().WithParameterPrefix($"b{index}_");

            sql.AppendLine(query.Sql);

            foreach (var name in query.Parameters.ParameterNames)
                parameters.Add(name, query.Parameters.Get<object?>(name));
        }

        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition(sql.ToString(), parameters, cancellationToken: ct));

        return read(multi);
    }
}
