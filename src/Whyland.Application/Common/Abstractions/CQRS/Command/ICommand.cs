using Whyland.Shared.Results;

namespace Whyland.Application.Common.Abstractions.CQRS.Command;

/// <summary>
/// Represents a command with no specific return value
/// (other than the operation <see cref="Result"/>).
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Represents a command that returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
public interface ICommand<TResult> : IRequest<Result<TResult>>;