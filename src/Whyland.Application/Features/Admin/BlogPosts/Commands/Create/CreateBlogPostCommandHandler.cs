using Whyland.Domain.Model.Blog.BlogPost.Errors;

namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Create;

/// <summary>
/// Handles <see cref="CreateBlogPostCommand"/> by checking the cross-row references and slug,
/// storing the optional cover image, creating the post through its domain factory, and persisting it
/// through the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to blog post persistence.</param>
/// <param name="imageStorageService">Stores and removes the optional cover image.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class CreateBlogPostCommandHandler(
    IUnitOfWork unitOfWork,
    IImageStorageService imageStorageService,
    ILogger<CreateBlogPostCommandHandler> logger) : ICommandHandler<CreateBlogPostCommand, BlogPostId>
{
    /// <summary>
    /// Creates a blog post, or returns an error when a reference is unavailable, the slug is taken,
    /// the domain rejects the values, the image cannot be stored, or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new post values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created post, or an error describing the failure.</returns>
    public async Task<Result<BlogPostId>> Handle(CreateBlogPostCommand command, CancellationToken ct)
    {
        var references = await BlogPostReferenceValidator.ValidateAsync(
            unitOfWork, command.AuthorId, command.CategoryId, ct);
        if (references.IsFailure)
            return references.Error;

        var slug = command.Slug.Trim().ToLowerInvariant();
        if (await unitOfWork.BlogPostCommandRepository.IsSlugTakenAsync(slug, ct: ct))
            return BlogPostErrors.SlugTaken(slug);

        var created = BlogPost.Create(
            command.Title,
            command.ShortDescription,
            command.Content,
            command.AuthorId,
            command.CategoryId,
            slug,
            command.DisplayOrder,
            command.DisplayStatus);

        if (created.IsFailure)
            return created.Error;

        var post = created.Data!;
        StoredImage? storedImage = null;

        if (command.Image is not null)
        {
            var saved = await BlogPostImageStore.SaveAsync(imageStorageService, command.Image, ct);
            if (saved.IsFailure)
                return saved.Error;

            storedImage = saved.Data!;

            var imageSet = post.SetImage(storedImage.RelativePath);
            if (imageSet.IsFailure)
            {
                await BlogPostImageStore.DeleteAsync(imageStorageService, storedImage.RelativePath, ct);
                return imageSet.Error;
            }
        }

        try
        {
            await unitOfWork.BlogPostCommandRepository.AddAsync(post, ct);

            var saveResult = await unitOfWork.SaveAsync(ct);
            if (saveResult.IsFailure)
            {
                await BlogPostImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);
                return BlogPostErrors.CreateFailed();
            }

            // Compact the whole listing to 1..n so an arbitrary order typed in the form still lands
            // on the sequence slot matching the post's sorted position.
            await unitOfWork.BlogPostCommandRepository.RenumberAsync(ct);
            saveResult = await unitOfWork.SaveAsync(ct);

            if (saveResult.IsFailure)
                return BlogPostErrors.CreateFailed();

            return post.Id;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Creating blog post '{Slug}' failed.", slug);

            await BlogPostImageStore.DeleteAsync(imageStorageService, storedImage?.RelativePath, ct);
            return BlogPostErrors.CreateFailed();
        }
    }
}
