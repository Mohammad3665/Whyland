using CategoryEntity = Whyland.Domain.Model.Catalog.Category.Entities.Category;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Category;

/// <summary>
/// The EF Core command repository for the <c>Category</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
public class CategoryCommandRepository(WhylandDbContext context) : ICategoryCommandRepository
{
    #region Sibling groups

    /// <summary>
    /// The value a null <see cref="CategoryEntity.ParentId"/> is folded into when rows are grouped by
    /// parent. Roots need a group key of their own, and comparing a nullable column to a nullable
    /// parameter would compile to <c>"ParentId" = @p</c>, which never matches SQL NULL — so every
    /// root-level comparison silently returns nothing. A sentinel makes the comparison total.
    /// </summary>
    private static readonly CategoryId NoParent = CategoryId.From(0L);

    #endregion

    #region Live rows

    /// <summary>
    /// The rows the catalogue still shows. This table has no global query filter, so without it a
    /// soft-deleted category would keep occupying its name slot inside its sibling group and take
    /// part in the renumbering.
    /// </summary>
    private IQueryable<CategoryEntity> Live => context.Categories.Where(x => !x.IsDeleted);

    #endregion

    #region Reads

    public Task<CategoryEntity?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var key = CategoryId.From(id);
        return Live.FirstOrDefaultAsync(x => x.Id == key, ct);
    }

    public async Task<IReadOnlyList<CategoryEntity>> GetLiveAllAsync(CancellationToken ct = default)
        => await Live
            .OrderBy(x => x.ParentId ?? NoParent)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public Task<bool> IsNameTakenAsync(
        string name,
        long? parentId,
        long? excludeId = null,
        CancellationToken ct = default)
    {
        // Raw request numbers become identifiers here, once, so the query below compares keys.
        var group = parentId is { } parent ? CategoryId.From(parent) : NoParent;
        var excluded = excludeId is { } skip ? CategoryId.From(skip) : (CategoryId?)null;

        return Live.AsNoTracking()
            .AnyAsync(
                x => x.Name == name &&
                     (x.ParentId ?? NoParent) == group &&
                     (excluded == null || x.Id != excluded),
                ct);
    }

    #endregion

    #region Writes

    public async Task AddAsync(CategoryEntity category, CancellationToken ct = default)
        => await context.Categories.AddAsync(category, ct);

    public void Update(CategoryEntity category)
        => context.Entry(category).State = EntityState.Modified;

    public void SoftDelete(CategoryEntity category) => category.MarkDeleted();

    public async Task<int> DeactivateDescendantsAsync(long id, CancellationToken ct = default)
    {
        // One read of the whole tree, then a walk of it: the depth is capped at three levels, so
        // this is a handful of rows and no recursive SQL.
        var all = await Live.ToListAsync(ct);
        var byParent = all.ToLookup(x => x.ParentId ?? NoParent);

        var root = CategoryId.From(id);
        var pending = new Queue<CategoryId>([root]);
        var seen = new HashSet<CategoryId> { root };
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

    public async Task ReorderAsync(IReadOnlyList<long> orderedIds, CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var ids = orderedIds.Select(CategoryId.From).ToList();

        var rows = await Live
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        // The next free slot per sibling group, in the order the groups appear in the visual list.
        var nextOrderByGroup = new Dictionary<CategoryId, int>();

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
