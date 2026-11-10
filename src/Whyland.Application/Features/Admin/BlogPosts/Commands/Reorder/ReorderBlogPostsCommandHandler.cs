namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Reorder;

/// <summary>
/// Handles <see cref="ReorderBlogPostsCommand"/> by applying the target display-order slots and
/// persisting them through the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog post persistence.</param>
public sealed class ReorderBlogPostsCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderBlogPostsCommand>
{
    /// <summary>
    /// Persists the new display order, or returns an error when persistence fails.
    /// </summary>
    /// <param name="command">The command carrying the new ordering.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(ReorderBlogPostsCommand command, CancellationToken ct)
    {
        await unitOfWork.BlogPostCommandRepository.ReorderAsync(command.Ordering, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? BlogPostErrors.ReorderFailed() : Result.Success();
    }
}
