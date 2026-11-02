using CategoryEntity = Whyland.Domain.Entities.Catalog.Category;

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
    private const long NoParent = 0L;

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
        => Live.FirstOrDefaultAsync(x => x.Id == id, ct);

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
        var group = parentId ?? NoParent;

        return Live.AsNoTracking()
            .AnyAsync(
                x => x.Name == name &&
                     (x.ParentId ?? NoParent) == group &&
                     (excludeId == null || x.Id != excludeId.Value),
                ct);
    }

    #endregion

    #region Writes

    public async Task AddAsync(CategoryEntity category, CancellationToken ct = default)
        => await context.Categories.AddAsync(category, ct);

    public void Update(CategoryEntity category)
        => context.Entry(category).State = EntityState.Modified;

    public void SoftDelete(CategoryEntity category)
    {
        category.IsDeleted = true;
        category.DeletedAt ??= DateTime.UtcNow;
    }

    public async Task<int> DeactivateDescendantsAsync(long id, CancellationToken ct = default)
    {
        // One read of the whole tree, then a walk of it: the depth is capped at three levels, so
        // this is a handful of rows and no recursive SQL.
        var all = await Live.ToListAsync(ct);
        var byParent = all.ToLookup(x => x.ParentId ?? NoParent);

        var pending = new Queue<long>([id]);
        var seen = new HashSet<long> { id };
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

        var rows = await Live
            .Where(x => orderedIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        // The next free slot per sibling group, in the order the groups appear in the visual list.
        var nextOrderByGroup = new Dictionary<long, int>();

        foreach (var id in orderedIds)
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
