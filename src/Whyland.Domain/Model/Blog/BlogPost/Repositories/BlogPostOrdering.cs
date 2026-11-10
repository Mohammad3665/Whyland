namespace Whyland.Domain.Model.Blog.BlogPost.Repositories;

/// <summary>
/// One row of a drag &amp; drop reorder: the post and the display-order slot it should occupy.
/// </summary>
/// <param name="Id">The blog post identifier.</param>
/// <param name="DisplayOrder">The target position, starting at one.</param>
public sealed record BlogPostOrdering(BlogPostId Id, int DisplayOrder);
