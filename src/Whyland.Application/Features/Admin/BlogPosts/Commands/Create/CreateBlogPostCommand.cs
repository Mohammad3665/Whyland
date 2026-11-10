namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Create;

/// <summary>
/// Command that creates a new blog post and returns the identifier of the created row.
/// A cover image is optional; when present it is stored before the row is saved.
/// </summary>
/// <param name="Title">The post title.</param>
/// <param name="ShortDescription">The excerpt shown in listings.</param>
/// <param name="Content">The full article body from the rich-text editor.</param>
/// <param name="AuthorId">The authoring account.</param>
/// <param name="CategoryId">The category the post is filed under.</param>
/// <param name="Slug">The unique URL-safe slug.</param>
/// <param name="DisplayOrder">The ordering weight in the article listing.</param>
/// <param name="DisplayStatus">The public visibility status.</param>
/// <param name="Image">The optional cover image upload.</param>
public sealed record CreateBlogPostCommand(
    string Title,
    string ShortDescription,
    string Content,
    UserId AuthorId,
    BlogCategoryId CategoryId,
    string Slug,
    int DisplayOrder,
    DisplayStatus DisplayStatus,
    BlogPostImageUpload? Image) : ICommand<BlogPostId>;
