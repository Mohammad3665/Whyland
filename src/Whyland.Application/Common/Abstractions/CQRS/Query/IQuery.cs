namespace Whyland.Application.Common.Abstractions.CQRS.Query;

/// <summary>
/// Represents a query that returns a result of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the data returned by the query. Must be non-nullable.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull;