using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseFaq;

/// <summary>
/// The EF Core command repository for the <c>CourseFaq</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work. Ordering is always
/// scoped to one course, so reordering a question never renumbers another course's FAQ list.
/// </summary>
/// <param name="context">The scoped EF Core context the repository stages changes on.</param>
public sealed class CourseFaqCommandRepository(WhylandDbContext context) : ICourseFaqCommandRepository
{
    /// <summary>Stages a new question; persisted on SaveAsync.</summary>
    /// <param name="faq">The question to add.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task AddAsync(CourseFaqEntity faq, CancellationToken ct = default)
        => await context.CourseFaqs.AddAsync(faq, ct);

    /// <summary>Returns the tracked question, or null if not found.</summary>
    /// <param name="id">Course FAQ identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<CourseFaqEntity?> GetByIdAsync(CourseFaqId id, CancellationToken ct = default)
        => context.CourseFaqs.FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <summary>Stages an existing question for update; persisted on SaveAsync.</summary>
    /// <param name="faq">The tracked question to update.</param>
    public void Update(CourseFaqEntity faq)
        => context.Entry(faq).State = EntityState.Modified;

    /// <summary>Marks the question as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="faq">The tracked question to soft-delete.</param>
    public void SoftDelete(CourseFaqEntity faq) => faq.MarkDeleted();

    /// <summary>Stages the new display-order sequence for one course; persisted on SaveAsync.</summary>
    /// <param name="courseId">The course whose FAQ list is reordered.</param>
    /// <param name="orderedIds">The question ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ReorderAsync(
        CourseId courseId,
        IReadOnlyList<CourseFaqId> orderedIds,
        CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var ids = orderedIds.ToList();

        // Load only the affected, live rows; unknown ids are silently skipped.
        var rows = await context.CourseFaqs
            .Where(x => x.CourseId == courseId && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < ids.Count; i++)
            if (rows.TryGetValue(ids[i], out var faq))
                faq.SetDisplayOrder(i + 1);
    }

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence for one course (global query
    /// filter excludes soft-deleted rows); persisted on SaveAsync.
    /// </summary>
    /// <param name="courseId">The course whose FAQ list is renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RenumberAsync(CourseId courseId, CancellationToken ct = default)
    {
        var course = courseId;

        var rows = await context.CourseFaqs
            .Where(x => x.CourseId == course)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < rows.Count; i++)
            rows[i].SetDisplayOrder(i + 1);
    }
}
