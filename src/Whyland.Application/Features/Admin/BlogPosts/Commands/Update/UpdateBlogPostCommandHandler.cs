using Whyland.Domain.Model.Blog.BlogPost.Errors;

namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateBlogPostCommand"/> by loading the tracked post, checking the references
/// and slug, applying the domain update, storing a replacement cover image when one was uploaded,
/// and persisting the result.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog post persistence.</param>
/// <param name="imageStorageService">Stores and removes the optional cover image.</param>
/// <param name="logger">The logger used to record cleanup failures and masked persistence failures.</param>
public sealed class UpdateBlogPostCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<UpdateBlogPostCommandHandler> logger) : ICommandHandler<UpdateBlogPostCommand>
{
    /// <summary>
    /// Updates the requested post, or returns an error when it no longer exists, a reference is
    /// unavailable, the slug is taken, the domain rejects the values, the image cannot be stored,
    /// or persistence fails.
    /// </summary>
    /// <param name="command">The command with the updated post values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateBlogPostCommand command, CancellationToken ct)
    {
        var repository = unitOfWork.BlogPostCommandRepository;

        var post = await repository.GetByIdAsync(command.Id, ct);
        if (post is null)
            return BlogPostErrors.NotFound(command.Id);

        // The post keeps the account that wrote it. Validation still checks that the stored author
        // exists, but the command has no way to replace it; UpdatedBy is stamped by the audit
        // interceptor from the signed-in editor.
        var references = await BlogPostReferenceValidator.ValidateAsync(
            unitOfWork, post.AuthorId, command.CategoryId, ct, post.CategoryId);
        if (references.IsFailure)
            return references.Error;

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await repository.IsSlugTakenAsync(slug, command.Id, ct))
            return BlogPostErrors.SlugTaken(slug);

        var updated = post.Update(
            command.Title,
            command.ShortDescription,
            command.Content,
            post.AuthorId,
            command.CategoryId,
            slug,
            command.DisplayOrder,
            command.DisplayStatus);

        if (updated.IsFailure)
            return updated.Error;

        var oldImage = post.Image;
        StoredImage? newImage = null;

        if (command.Image is not null)
        {
            var saved = await BlogPostImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            newImage = saved.Data!;

            var imageSet = post.SetImage(newImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await BlogPostImageStore.DeleteAsync(imageStorageService, newImage.RelativePath, ct);
                return imageSet.Error;
            }
        }
        else if (command.RemoveImage)
        {
            var imageCleared = post.SetImage(null);
            if (imageCleared.IsFailure)
                return imageCleared.Error;
        }

        repository.Update(post);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
        {
            await BlogPostImageStore.DeleteAsync(imageStorageService, newImage?.RelativePath, ct);
            return BlogPostErrors.UpdateFailed();
        }

        // Compact the listing's display order after an order edit, then persist the numbering.
        await repository.RenumberAsync(ct);
        saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return BlogPostErrors.UpdateFailed();

        // The row now points at the new file (or at nothing), so the previous file is unreachable.
        // A failure here costs disk space, never data, so it is logged and the save stands.
        if (newImage is not null || command.RemoveImage)
            await DeleteOldImageAsync(oldImage, ct);

        return Result.Success();
    }

    private async Task DeleteOldImageAsync(string? oldImage, CancellationToken ct)
    {
        var deleted = await BlogPostImageStore.DeleteAsync(imageStorageService, oldImage, ct);
        if (deleted.IsFailure)
            logger.LogWarning("The old blog post image '{Path}' could not be deleted.", oldImage);
    }
}
