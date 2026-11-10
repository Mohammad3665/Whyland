namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Update;

/// <summary>
/// Command that updates an existing blog post: its content and metadata, its category, its position
/// and state, and — when a new file was chosen — its cover image. The author is deliberately absent:
/// it belongs to the account that created the post and is never rewritten by an editor.
/// </summary>
/// <param name="Id">The identifier of the post to update.</param>
/// <param name="Title">The new title.</param>
/// <param name="ShortDescription">The new listing excerpt.</param>
/// <param name="Content">The new article body.</param>
/// <param name="CategoryId">The new category.</param>
/// <param name="Slug">The new unique URL segment.</param>
/// <param name="DisplayOrder">The new ordering weight in the article listing.</param>
/// <param name="DisplayStatus">The new visibility status.</param>
/// <param name="RemoveImage">Whether the existing cover image should be removed.</param>
/// <param name="Image">The uploaded replacement/cover image, when one was chosen.</param>
public sealed record UpdateBlogPostCommand(
    BlogPostId Id,
    string Title,
    string ShortDescription,
    string Content,
    BlogCategoryId CategoryId,
    string Slug,
    int DisplayOrder,
    DisplayStatus DisplayStatus,
    bool RemoveImage,
    BlogPostImageUpload? Image) : ICommand;
