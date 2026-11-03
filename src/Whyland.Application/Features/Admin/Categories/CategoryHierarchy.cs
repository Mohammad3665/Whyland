namespace Whyland.Application.Features.Admin.Categories;

/// <summary>
/// The three-level ceiling of the catalogue is judged on the whole branch, not on one row, so both
/// the create and the update handler have to walk the tree. They read it out of one live listing
/// and this class owns the walks, so the two handlers cannot disagree about what a level means.
/// </summary>
internal static class CategoryHierarchy
{
    /// <summary>
    /// The group key a root category belongs to: <see cref="Category.ParentId"/> is null there and
    /// the walk below compares identifiers, so null is folded into this sentinel.
    /// </summary>
    internal static readonly CategoryId NoParent = CategoryId.From(0L);

    /// <summary>
    /// The level a category sits on: a root is 1, its child 2, and so on. An identifier that is not
    /// in the listing stops the walk, so a broken reference returns the depth reached instead of
    /// throwing.
    /// </summary>
    /// <param name="id">The category whose level is wanted.</param>
    /// <param name="byId">Every live category, keyed by identifier.</param>
    internal static int DepthOf(CategoryId id, IReadOnlyDictionary<CategoryId, Category> byId)
    {
        var depth = 0;
        var current = id;

        // The guard is the cycle breaker: a corrupted row could otherwise send this walk round forever.
        while (current != NoParent && depth <= Category.MaxDepth)
        {
            if (!byId.TryGetValue(current, out var row))
                break;

            depth++;
            current = row.ParentId ?? NoParent;
        }

        return depth;
    }

    /// <summary>
    /// How many levels the branch rooted at <paramref name="id"/> occupies, counting the root itself
    /// as one. Used when a whole branch is about to be moved under another parent.
    /// </summary>
    /// <param name="id">The top of the branch.</param>
    /// <param name="byParent">Every live category, grouped by its parent (roots under <see cref="NoParent"/>).</param>
    /// <param name="limit">Stops the walk on a corrupted cycle.</param>
    internal static int HeightOf(CategoryId id, ILookup<CategoryId, Category> byParent, int limit = Category.MaxDepth)
    {
        if (limit <= 1)
            return 1;

        var height = 1;
        foreach (var child in byParent[id])
            height = Math.Max(height, 1 + HeightOf(child.Id, byParent, limit - 1));

        return height;
    }

    /// <summary>
    /// Whether a parent can still take one more child. The row is counted out of the same live
    /// listing the depth checks walk, so the two rules can never disagree about which children
    /// exist — and a row being edited must be excluded by the caller, since it already occupies
    /// one of the slots it is being counted against.
    /// </summary>
    /// <param name="all">Every live category.</param>
    /// <param name="parentId">The parent whose children are counted.</param>
    internal static bool HasRoomForChild(IReadOnlyList<Category> all, CategoryId parentId) =>
        all.Count(x => x.ParentId == parentId) < Category.MaxChildren;

    /// <summary>
    /// Whether <paramref name="candidateId"/> hangs somewhere below <paramref name="ancestorId"/>,
    /// which is what makes a parent change into a loop.
    /// </summary>
    /// <param name="candidateId">The row proposed as the new parent.</param>
    /// <param name="ancestorId">The row that would become its own descendant.</param>
    /// <param name="byId">Every live category, keyed by identifier.</param>
    internal static bool IsDescendant(
        CategoryId candidateId,
        CategoryId ancestorId,
        IReadOnlyDictionary<CategoryId, Category> byId)
    {
        var current = candidateId;

        while (byId.TryGetValue(current, out var row) && row.ParentId is { } parentId)
        {
            if (parentId == ancestorId)
                return true;

            current = parentId;
        }

        return false;
    }
}
