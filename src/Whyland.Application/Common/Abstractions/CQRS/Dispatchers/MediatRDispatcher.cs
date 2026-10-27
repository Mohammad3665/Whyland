namespace Whyland.Application.Common.Abstractions.CQRS.Dispatchers;

internal sealed class MediatRDispatcher(ISender sender) : IDispatcher
{
    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
        => sender.Send(command, cancellationToken);

    public Task<Result<TData>> Send<TData>(ICommand<TData> command, CancellationToken cancellationToken = default)
        => sender.Send(command, cancellationToken);

    public Task<Result<TData>> Send<TData>(IQuery<TData> query, CancellationToken cancellationToken = default)
        where TData : notnull
        => sender.Send(query, cancellationToken);
}