using System;
using Whyland.Domain.IRepositories.Content.Faq;

namespace Whyland.Infrastructure.Persistence.Repositories.Content.Faq;

/// <summary>
/// The EF Core command repository for the <c>Faq</c> aggregate, backed by the shared
/// scoped <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
public class FaqCommandRepository(WhylandDbContext context) : IFaqCommandRepository
{
    public async Task AddAsync(Domain.Entities.Content.Faq faq, CancellationToken ct = default)
        => await context.Faqs.AddAsync(faq, ct);

    public Task<bool> ExistsAsync(string title, long? excludeId = null, CancellationToken ct = default)
        => context.Faqs.AsNoTracking()
            .AnyAsync(x => x.Title == title && (excludeId == null || x.Id != excludeId.Value), ct);

    public Task<Domain.Entities.Content.Faq?> GetByIdAsync(long id, CancellationToken ct = default)
        => context.Faqs.FirstOrDefaultAsync(x => x.Id == id, ct);

    public void SoftDelete(Domain.Entities.Content.Faq faq)
    {
        faq.IsDeleted = true;
        faq.DeletedAt ??= DateTime.UtcNow;
    }

    public void Update(Domain.Entities.Content.Faq faq)
        => context.Entry(faq).State = EntityState.Modified;

    /// <summary>Stages the new display-order sequence; persisted on SaveAsync.</summary>
    /// <param name="orderedIds">The Faq ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ReorderAsync(IReadOnlyList<long> orderedIds, CancellationToken ct = default)
    {
        // Load only the affected, live rows; unknown ids are silently skipped.
        if (orderedIds.Count == 0)
            return;

        var rows = await context.Faqs
            .Where(x => orderedIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < orderedIds.Count; i++)
            if (rows.TryGetValue(orderedIds[i], out var faq))
                faq.DisplayOrder = i + 1;
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
            faqs[i].DisplayOrder = i + 1;
    }
}
