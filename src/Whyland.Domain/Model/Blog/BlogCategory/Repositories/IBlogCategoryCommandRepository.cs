using BlogCategoryEntity = Whyland.Domain.Model.Blog.BlogCategory.Entities.BlogCategory;

namespace Whyland.Domain.Model.Blog.BlogCategory.Repositories;

/// <summary>
/// Command-side repository for the BlogCategory aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IBlogCategoryCommandRepository
{
    #region Reads

    /// <summary>Returns the tracked blog category, or null when there is none with that identifier.</summary>
    /// <param name="id">Blog category identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<BlogCategoryEntity?> GetByIdAsync(BlogCategoryId id, CancellationToken ct = default);

    /// <summary>
    /// Returns every live blog category, tracked. The handlers walk the tree out of this single read —
    /// the loop check, the cascade that disables a subtree and the renumbering all need the whole
    /// taxonomy at once.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<BlogCategoryEntity>> GetLiveAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Whether another live blog category already uses this slug. The slug is the segment the public
    /// blog URL is built from, so it is unique across the whole taxonomy rather than per sibling group.
    /// </summary>
    /// <param name="slug">The slug to look for, already trimmed and lower-cased.</param>
    /// <param name="excludeId">A row that must not count against itself, when editing.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsSlugTakenAsync(
        string slug,
        BlogCategoryId? excludeId = null,
        CancellationToken ct = default);

    /// <summary>
    /// How many live posts are filed under the category. Deleting it while this is above zero would
    /// orphan the post-category relation, which s12.1.1 forbids.
    /// </summary>
    /// <param name="id">The category whose posts are counted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<int> CountLivePostsAsync(BlogCategoryId id, CancellationToken ct = default);

    #endregion

    #region Writes

    /// <summary>Stages a new blog category; persisted on SaveAsync.</summary>
    /// <param name="category">The category to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(BlogCategoryEntity category, CancellationToken ct = default);

    /// <summary>Stages an existing blog category for update; persisted on SaveAsync.</summary>
    /// <param name="category">The tracked category to update.</param>
    void Update(BlogCategoryEntity category);

    /// <summary>Marks the blog category as soft-deleted, keeping the row for its audit trail.</summary>
    /// <param name="category">The tracked category to soft-delete.</param>
    void SoftDelete(BlogCategoryEntity category);

    /// <summary>
    /// Switches off every live descendant of <paramref name="id"/>: a disabled category is not
    /// selectable for new posts, so leaving its children enabled would publish branches whose parent
    /// is hidden. The posts already filed under them are untouched — s12.1.1 forbids dropping an
    /// existing relation as a side effect of deactivation. The change is staged only; persisted on SaveAsync.
    /// </summary>
    /// <param name="id">The identifier of the category whose subtree is being switched off.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many descendants were switched off.</returns>
    Task<int> DeactivateDescendantsAsync(BlogCategoryId id, CancellationToken ct = default);

    #endregion

    #region Ordering

    /// <summary>
    /// Stages the visual order a drag &amp; drop produced. The identifiers are walked in the order
    /// they now appear on the table and each one is numbered inside <b>its own</b> sibling group,
    /// because DisplayOrder counts between siblings — a flat 1..n over the whole list would scramble
    /// every group to fix one. Unknown identifiers are skipped.
    /// </summary>
    /// <param name="orderedIds">The category identifiers in the new visual order.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(IReadOnlyList<BlogCategoryId> orderedIds, CancellationToken ct = default);

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence inside every sibling group
    /// (the root level counts as a group of its own), so a gap left by a delete closes and the
    /// numbers the client never sent stay in step.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CancellationToken ct = default);

    #endregion
}