using Whyland.Application.Common.Interfaces.Localization;

namespace Whyland.Application.Common.Behaviors;

/// <summary>
/// A MediatR pipeline behavior that puts the user-facing sentence on a failed result, looked up from the
/// error code its handler or validator raised.
/// <para>
/// Layers below the edge pass codes only, so wording lives in one catalog instead of every handler and view.
/// Pages keep reading <c>Result.Error.Description</c> and never learn that the text was resolved.
/// </para>
/// </summary>
/// <typeparam name="TRequest">The type of the request being handled.</typeparam>
/// <typeparam name="TResponse">The result type returned by the handler; <see cref="Result"/> or <see cref="Result{TData}"/>.</typeparam>
public class ErrorLocalizationBehavior<TRequest, TResponse>(IErrorLocalizer localizer)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : BaseResult
{
    /// <summary>
    /// Runs the inner pipeline, then describes the failure it produced. A result that already carries a
    /// description — one authored at the edge rather than by a handler — is returned untouched.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The delegate that invokes the next behavior or the handler.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response, with <see cref="Error.Description"/> filled in when the operation failed.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        return response is { IsFailure: true, Error.Description: null }
            ? ResultFactory.Failure<TResponse>(localizer.Localize(response.Error))
            : response;
    }
}
