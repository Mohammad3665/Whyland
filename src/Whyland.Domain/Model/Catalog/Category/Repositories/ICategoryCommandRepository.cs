using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;

namespace Whyland.Domain.Model.Catalog.Category.Repositories;

/// <summary>
/// Command-side repository for the Category aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface ICategoryCommandRepository
{
    #region Reads

    /// <summary>Returns the tracked category, or null when there is none with that identifier.</summary>
    /// <param name="id">Category identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<CategoryEntity?> GetByIdAsync(CategoryId id, CancellationToken ct = default);

    /// <summary>
    /// Returns every live category, tracked. The handlers walk the tree out of this single read —
    /// depth checks, the cascade that disables a subtree and the renumbering all need the whole
    /// catalogue at once, and it is small enough to read in one go.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<CategoryEntity>> GetLiveAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Whether another live category already uses this name inside the same sibling group. Two
    /// same-named siblings would be indistinguishable in the catalogue, while the same name under a
    /// different parent is normal («دیتابیس» under both «برنامه‌نویسی» and «مهندسی» is fine).
    /// </summary>
    /// <param name="name">The name to look for.</param>
    /// <param name="parentId">The sibling group to look in; null means the root level.</param>
    /// <param name="excludeId">A row that must not count against itself, when editing.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsNameTakenAsync(
        string name,
        CategoryId? parentId,
        CategoryId? excludeId = null,
        CancellationToken ct = default);

    #endregion

    #region Writes

    /// <summary>Stages a new category; persisted on SaveAsync.</summary>
    /// <param name="category">The category to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(CategoryEntity category, CancellationToken ct = default);

    /// <summary>Stages an existing category for update; persisted on SaveAsync.</summary>
    /// <param name="category">The tracked category to update.</param>
    void Update(CategoryEntity category);

    /// <summary>Marks the category as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="category">The tracked category to soft-delete.</param>
    void SoftDelete(CategoryEntity category);

    /// <summary>
    /// Switches off every live descendant of <paramref name="id"/>: a disabled category is hidden on
    /// the site, so leaving its children enabled would publish branches whose parent is missing
    /// (s7.1.3). The change is staged only; persisted on SaveAsync.
    /// </summary>
    /// <param name="id">The identifier of the category whose subtree is being switched off.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many descendants were switched off.</returns>
    Task<int> DeactivateDescendantsAsync(CategoryId id, CancellationToken ct = default);

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
    Task ReorderAsync(IReadOnlyList<CategoryId> orderedIds, CancellationToken ct = default);

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence inside every sibling group
    /// (the root level counts as a group of its own), so a gap left by a delete closes and the
    /// numbers the client never sent stay in step.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CancellationToken ct = default);

    #endregion
}
