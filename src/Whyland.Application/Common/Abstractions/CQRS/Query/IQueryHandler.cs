namespace Whyland.Application.Common.Abstractions.CQRS.Query;

/// <summary>
/// Defines a handler for a query that returns a result of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TQuery">The type of the query being handled.</typeparam>
/// <typeparam name="TResponse">The type of the data returned by the query. Must be non-nullable.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IRequest<Result<TResponse>>
    where TResponse : notnull;