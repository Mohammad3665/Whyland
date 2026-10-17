namespace Whyland.Application.Common.Abstractions.CQRS.Command;

/// <summary>
/// Defines a handler for a command that does not return a specific value
/// (other than the operation <see cref="Result"/>).
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Defines a handler for a command that returns a result of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command being handled.</typeparam>
/// <typeparam name="TResponse">The type of the result returned by the command.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;