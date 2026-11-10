using BlogCategoryEntity = Whyland.Domain.Model.Blog.BlogCategory.Entities.BlogCategory;

namespace Whyland.Infrastructure.Persistence.Repositories.Blog.BlogCategory;

/// <summary>
/// The EF Core command repository for the <c>BlogCategory</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
public class BlogCategoryCommandRepository(WhylandDbContext context) : IBlogCategoryCommandRepository
{
    #region Sibling groups

    /// <summary>
    /// The value a null <see cref="BlogCategoryEntity.ParentId"/> is folded into when rows are grouped by
    /// parent. Roots need a group key of their own, and comparing a nullable column to a nullable
    /// parameter would compile to <c>"ParentId" = @p</c>, which never matches SQL NULL — so every
    /// root-level comparison silently returns nothing. A sentinel makes the comparison total.
    /// </summary>
    private static readonly BlogCategoryId NoParent = BlogCategoryId.From(0L);

    #endregion

    #region Live rows

    /// <summary>
    /// The rows the taxonomy still shows. The global soft-delete filter already hides the deleted ones;
    /// naming the set here keeps every read below explicit about which rows take part in the tree walks
    /// and in the renumbering.
    /// </summary>
    private IQueryable<BlogCategoryEntity> Live => context.BlogCategories.Where(x => !x.IsDeleted);

    #endregion

    #region Reads

    public Task<BlogCategoryEntity?> GetByIdAsync(BlogCategoryId id, CancellationToken ct = default)
        => Live.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<BlogCategoryEntity>> GetLiveAllAsync(CancellationToken ct = default)
        => await Live
            .OrderBy(x => x.ParentId ?? NoParent)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public Task<bool> IsSlugTakenAsync(
        string slug,
        BlogCategoryId? excludeId = null,
        CancellationToken ct = default)
    {
        var excluded = excludeId is { } skip ? skip : (BlogCategoryId?)null;

        return Live.AsNoTracking()
            .AnyAsync(
                x => x.Slug == slug && (excluded == null || x.Id != excluded),
                ct);
    }

    public Task<int> CountLivePostsAsync(BlogCategoryId id, CancellationToken ct = default)
        => context.BlogPosts.CountAsync(x => x.CategoryId == id, ct);

    #endregion

    #region Writes

    public async Task AddAsync(BlogCategoryEntity category, CancellationToken ct = default)
        => await context.BlogCategories.AddAsync(category, ct);

    public void Update(BlogCategoryEntity category)
        => context.Entry(category).State = EntityState.Modified;

    public void SoftDelete(BlogCategoryEntity category) => category.MarkDeleted();

    public async Task<int> DeactivateDescendantsAsync(BlogCategoryId id, CancellationToken ct = default)
    {
        // One read of the whole tree, then a walk of it. The blog taxonomy has no depth ceiling
        // (s12.1.1), so the walk is a queue over the sibling groups rather than a fixed number of
        // levels, and the visited set is what keeps a corrupted cycle from looping forever.
        var all = await Live.ToListAsync(ct);
        var byParent = all.ToLookup(x => x.ParentId ?? NoParent);

        var root = id;
        var pending = new Queue<BlogCategoryId>([root]);
        var seen = new HashSet<BlogCategoryId> { root };
        var count = 0;

        while (pending.Count > 0)
        {
            foreach (var child in byParent[pending.Dequeue()])
            {
                if (!seen.Add(child.Id))
                    continue;

                if (child.IsActive)
                {
                    child.Deactivate();
                    count++;
                }

                pending.Enqueue(child.Id);
            }
        }

        return count;
    }

    #endregion

    #region Ordering

    public async Task ReorderAsync(IReadOnlyList<BlogCategoryId> orderedIds, CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var ids = orderedIds.ToList();

        var rows = await Live
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        // The next free slot per sibling group, in the order the groups appear in the visual list.
        var nextOrderByGroup = new Dictionary<BlogCategoryId, int>();

        foreach (var id in ids)
        {
            if (!rows.TryGetValue(id, out var category))
                continue;

            var group = category.ParentId ?? NoParent;
            var next = nextOrderByGroup.TryGetValue(group, out var last) ? last + 1 : 1;
            nextOrderByGroup[group] = next;

            category.SetDisplayOrder(next);
        }
    }

    public async Task RenumberAsync(CancellationToken ct = default)
    {
        var categories = await Live
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        foreach (var group in categories.GroupBy(x => x.ParentId ?? NoParent))
        {
            var order = 1;
            foreach (var category in group)
            {
                category.SetDisplayOrder(order++);
            }
        }
    }

    #endregion
}