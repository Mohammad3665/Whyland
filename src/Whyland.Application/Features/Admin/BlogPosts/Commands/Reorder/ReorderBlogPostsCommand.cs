using Whyland.Domain.Model.Blog.BlogPost.Repositories;

namespace Whyland.Application.Features.Admin.BlogPosts.Commands.Reorder;

/// <summary>
/// Command that persists the display order produced by dragging rows in the article table.
/// </summary>
/// <param name="Ordering">The posts and the display-order slot each one must take.</param>
public sealed record ReorderBlogPostsCommand(IReadOnlyList<BlogPostOrdering> Ordering) : ICommand;
