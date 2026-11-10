namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Delete;

/// <summary>
/// Command that soft-deletes an existing blog post by its identifier.
/// </summary>
/// <param name="Id">The identifier of the post to soft-delete.</param>
public sealed record DeleteBlogPostCommand(BlogPostId Id) : ICommand;
