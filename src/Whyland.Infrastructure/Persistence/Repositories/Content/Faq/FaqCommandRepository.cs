using FaqEntity = Whyland.Domain.Model.Content.Faq.Entities.Faq;

namespace Whyland.Infrastructure.Persistence.Repositories.Content.Faq;

/// <summary>
/// The EF Core command repository for the <c>Faq</c> aggregate, backed by the shared
/// scoped <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
public class FaqCommandRepository(WhylandDbContext context) : IFaqCommandRepository
{
    public async Task AddAsync(FaqEntity faq, CancellationToken ct = default)
        => await context.Faqs.AddAsync(faq, ct);

    public Task<bool> ExistsAsync(string title, FaqId? excludeId = null, CancellationToken ct = default)
    {
        var excluded = excludeId is { } skip ? skip : (FaqId?)null;

        return context.Faqs.AsNoTracking()
            .AnyAsync(x => x.Title == title && (excluded == null || x.Id != excluded.Value), ct);
    }

    public Task<FaqEntity?> GetByIdAsync(FaqId id, CancellationToken ct = default)
        => context.Faqs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void SoftDelete(FaqEntity faq) => faq.MarkDeleted();

    public void Update(FaqEntity faq)
        => context.Entry(faq).State = EntityState.Modified;

    /// <summary>Stages the new display-order sequence; persisted on SaveAsync.</summary>
    /// <param name="orderedIds">The Faq ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ReorderAsync(IReadOnlyList<FaqId> orderedIds, CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var ids = orderedIds.ToList();

        // Load only the affected, live rows; unknown ids are silently skipped.
        var rows = await context.Faqs
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < ids.Count; i++)
            if (rows.TryGetValue(ids[i], out var faq))
                faq.SetDisplayOrder(i + 1);
    }

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence over
    /// every live Faq (global query filter excludes soft-deleted rows);
    /// persisted on SaveAsync.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task RenumberAsync(CancellationToken ct = default)
    {
        var faqs = await context.Faqs
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < faqs.Count; i++)
            faqs[i].SetDisplayOrder(i + 1);
    }
}
