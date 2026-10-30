using FluentValidation.Results;

namespace Whyland.Application.Common.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that runs all <see cref="IValidator{T}"/> implementations
/// registered for <typeparamref name="TRequest"/> before the request reaches its handler.
/// <para>
/// If no validators are registered, the pipeline is short-circuited and the request
/// is passed straight to the next delegate.
/// </para>
/// <para>
/// If validation fails, the handler is never invoked. Instead, a failed
/// <typeparamref name="TResponse"/> is returned:
/// <list type="bullet">
///   <item><description>A single failure is surfaced as a single <see cref="Error"/>, preserving its code, field, and arguments.</description></item>
///   <item><description>Multiple failures are aggregated into one <see cref="Error"/> whose <c>Arguments["Errors"]</c> holds the full list.</description></item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="TRequest">The type of the request being validated.</typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the handler. Must derive from <see cref="BaseResult"/>
/// (<see cref="Result"/> or <see cref="Result{T}"/>).
/// </typeparam>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResult
{
    /// <summary>
    /// Validates the incoming request, then either invokes the next delegate
    /// (on success) or returns a failed result built from the validation failures.
    /// </summary>
    /// <param name="request">The request being validated.</param>
    /// <param name="next">The delegate that invokes the next behavior or the handler.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The handler's response when validation passes; otherwise a failed
    /// <typeparamref name="TResponse"/> carrying the validation errors.
    /// </returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered for this request — skip validation entirely.
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        // Run every validator concurrently and flatten the resulting errors.
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // Nothing failed — continue down the pipeline.
        if (failures.Count == 0)
            return await next(cancellationToken);

        return CreateValidationErrorResult<TResponse>(failures);
    }

    /// <summary>
    /// Builds a failure result from the collected validation failures.
    /// A single failure is returned as-is; multiple failures are collapsed into
    /// one aggregate error whose <c>Arguments["Errors"]</c> contains the full list.
    /// Neither carries wording: <see cref="ErrorLocalizationBehavior{TRequest,TResponse}"/> resolves the
    /// message from the code the rule was given.
    /// </summary>
    /// <typeparam name="TRes">The concrete result type to create (<see cref="Result"/> or <see cref="Result{T}"/>).</typeparam>
    /// <param name="failures">The non-empty list of validation failures.</param>
    /// <returns>A failed <typeparamref name="TRes"/> instance.</returns>
    private static TRes CreateValidationErrorResult<TRes>(List<ValidationFailure> failures)
        where TRes : BaseResult
    {
        Error error;

        if (failures.Count == 1)
        {
            // Single failure: preserve Code, Field, and Arguments as-is.
            error = ToError(failures[0]);
        }
        else
        {
            // Multiple failures: aggregate into a single error with the full list.
            var errors = failures.Select(ToError).ToList();

            error = new Error(
                Code: ValidationErrors.Failed.Code,
                Type: ErrorType.Validation,
                Arguments: new Dictionary<string, object?>
                {
                    ["Errors"] = errors
                });
        }

        return ResultFactory.Failure<TRes>(error);
    }

    /// <summary>
    /// Maps a FluentValidation <see cref="ValidationFailure"/> to an <see cref="Error"/>: the code the rule
    /// was given, the property it failed on, and any custom state as templating arguments.
    /// <para>
    /// <see cref="ValidationFailure.ErrorMessage"/> is left out on purpose — it is wording authored in the
    /// validator, and the localization catalog owns the sentence instead.
    /// <see cref="ValidationFailure.CustomState"/> is typed as <see cref="object"/>;
    /// it is only accepted when it is a dictionary of string keys to nullable objects.
    /// </para>
    /// </summary>
    /// <param name="failure">The validation failure to convert.</param>
    /// <returns>An <see cref="Error"/> of type <see cref="ErrorType.Validation"/>.</returns>
    private static Error ToError(ValidationFailure failure)
    {
        // CustomState is typed as object — safely cast to a dictionary.
        IReadOnlyDictionary<string, object?>? args = failure.CustomState switch
        {
            IReadOnlyDictionary<string, object?> d => d,
            IDictionary<string, object?> d => new Dictionary<string, object?>(d),
            _ => null
        };

        return new Error(
            Code: failure.ErrorCode,
            Type: ErrorType.Validation,
            Field: failure.PropertyName,
            Arguments: args);
    }
}