using BlogPostEntity = Whyland.Domain.Model.Blog.BlogPost.Entities.BlogPost;

namespace Whyland.Domain.Model.Blog.BlogPost.Repositories;

/// <summary>
/// Command-side repository for the BlogPost aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IBlogPostCommandRepository
{
    /// <summary>Returns the tracked blog post, or null when there is none with that identifier.</summary>
    /// <param name="id">Blog post identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BlogPostEntity?> GetByIdAsync(BlogPostId id, CancellationToken ct = default);

    /// <summary>
    /// Whether another live blog post already uses this slug.
    /// </summary>
    /// <param name="slug">The slug to look for, already trimmed and lower-cased.</param>
    /// <param name="excludeId">A row that must not count against itself, when editing.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsSlugTakenAsync(
        string slug,
        BlogPostId? excludeId = null,
        CancellationToken ct = default);

    /// <summary>Stages a new blog post; persisted on SaveAsync.</summary>
    /// <param name="post">The post to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(BlogPostEntity post, CancellationToken ct = default);

    /// <summary>Stages an existing blog post for update; persisted on SaveAsync.</summary>
    /// <param name="post">The tracked post to update.</param>
    void Update(BlogPostEntity post);

    /// <summary>Marks the blog post as soft-deleted, keeping the row for its audit trail.</summary>
    /// <param name="post">The tracked post to soft-delete.</param>
    void SoftDelete(BlogPostEntity post);

    /// <summary>
    /// Applies the display-order values produced by the drag &amp; drop table. Only the rows in the
    /// payload are touched; the caller sends the target slots for the rows it moved.
    /// </summary>
    /// <param name="ordering">The posts and the display-order slot each one must take.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(IReadOnlyList<BlogPostOrdering> ordering, CancellationToken ct = default);

    /// <summary>
    /// Compacts DisplayOrder to an unbroken 1..n sequence over every live post, so a gap left by a
    /// delete or an arbitrary number typed in the form closes.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CancellationToken ct = default);
}
