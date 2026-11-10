namespace Whyland.Application.Features.Admin.BlogCategories;

/// <summary>
/// The blog taxonomy has no depth ceiling (s12.1.1), so the only tree rule a save has to judge is
/// whether the new parent closes a loop. The handler reads the tree out of one live listing and this
/// class owns the walk, so the rule cannot be spelled two different ways.
/// </summary>
internal static class BlogCategoryHierarchy
{
    /// <summary>
    /// Whether <paramref name="candidateId"/> hangs somewhere below <paramref name="ancestorId"/>,
    /// which is what makes a parent change into a loop.
    /// </summary>
    /// <param name="candidateId">The row proposed as the new parent.</param>
    /// <param name="ancestorId">The row that would become its own descendant.</param>
    /// <param name="byId">Every live category, keyed by identifier.</param>
    internal static bool IsDescendant(
        BlogCategoryId candidateId,
        BlogCategoryId ancestorId,
        IReadOnlyDictionary<BlogCategoryId, BlogCategory> byId)
    {
        var current = candidateId;
        var seen = new HashSet<BlogCategoryId>();

        // With no ceiling to stop at, the visited set is the only cycle breaker: a corrupted row
        // could otherwise send this walk round forever.
        while (seen.Add(current) && byId.TryGetValue(current, out var row) && row.ParentId is { } parentId)
        {
            if (parentId == ancestorId)
                return true;

            current = parentId;
        }

        return false;
    }
}