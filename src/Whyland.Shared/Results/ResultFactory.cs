using Whyland.Shared.Errors;

namespace Whyland.Shared.Results;

/// <summary>
/// Builds a failed result for a response type that is only known at runtime, which is what a pipeline
/// behavior needs when it replaces the error carried by a handler's result.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Creates a failed <typeparamref name="TFailure"/> carrying <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="TFailure">The result type to build; <see cref="Result"/> or <see cref="Result{TData}"/>.</typeparam>
    /// <param name="error">The error the failure carries.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <typeparamref name="TFailure"/> is neither <see cref="Result"/> nor <see cref="Result{TData}"/>.
    /// </exception>
    public static TFailure Failure<TFailure>(Error error)
        where TFailure : BaseResult
    {
        if (typeof(TFailure) == typeof(Result))
            return (TFailure)(object)Result.Failure(error);

        var resultType = typeof(TFailure);

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var dataType = resultType.GetGenericArguments()[0];

            var failureMethod = typeof(Result<>).MakeGenericType(dataType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(Error)])
                ?? throw new InvalidOperationException($"Result<{dataType.Name}>.Failure(Error) not found.");

            return (TFailure)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Unsupported result type: {typeof(TFailure)}. {nameof(ResultFactory)} builds Result and Result<T> only.");
    }
}
