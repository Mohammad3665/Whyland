using Whyland.Domain.Model.Blog.BlogPost.Errors;

namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteBlogPostCommand"/> by soft-deleting the post through the command
/// repository and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog post persistence.</param>
public sealed class DeleteBlogPostCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteBlogPostCommand>
{
    /// <summary>
    /// Soft-deletes the requested post, or returns an error when it does not exist or persistence
    /// fails. The cover image is kept: the row remains in the audit trail and the file it points at is
    /// still owned by that row.
    /// </summary>
    /// <param name="command">The command containing the post identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteBlogPostCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.BlogPostCommandRepository;

        var post = await repository.GetByIdAsync(command.Id, ct);
        if (post is null)
            return BlogPostErrors.NotFound(command.Id);

        repository.SoftDelete(post);

        // The first save persists the soft delete so the compaction below does not include the
        // deleted row in its category's numbering.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return BlogPostErrors.SoftDeleteFailed();

        await repository.RenumberAsync(ct);
        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? BlogPostErrors.SoftDeleteFailed() : Result.Success();
    }
}
