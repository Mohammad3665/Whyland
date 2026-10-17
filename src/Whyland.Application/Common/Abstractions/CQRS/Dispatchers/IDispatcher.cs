using Whyland.Application.Common.Abstractions.CQRS.Command;
using Whyland.Application.Common.Abstractions.CQRS.Query;

namespace Whyland.Application.Common.Abstractions.CQRS.Dispatchers;

/// <summary>
/// Dispatches commands and queries to their corresponding handlers.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command that does not return a specific value
    /// (other than the operation <see cref="Result"/>).
    /// </summary>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result"/> representing the outcome of the operation.</returns>
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command that returns a result of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of the result returned by the command.</typeparam>
    /// <param name="command">The command to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result{TData}"/> containing the outcome and data of the operation.</returns>
    Task<Result<TData>> Send<TData>(ICommand<TData> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query that returns a result of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The type of the data returned by the query.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Result{TData}"/> containing the outcome and data of the query.</returns>
    Task<Result<TData>> Send<TData>(IQuery<TData> query, CancellationToken cancellationToken = default)
        where TData : notnull;
}