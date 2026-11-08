using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseSection;

/// <summary>
/// The EF Core command repository for the <c>CourseSection</c> aggregate, backed by the shared
/// scoped <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work. Ordering is
/// always scoped to one course, so reordering a section never renumbers another course's curriculum.
/// </summary>
/// <param name="context">The scoped EF Core context the repository stages changes on.</param>
public sealed class CourseSectionCommandRepository(WhylandDbContext context) : ICourseSectionCommandRepository
{
    /// <summary>Stages a new section; persisted on SaveAsync.</summary>
    /// <param name="section">The section to add.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task AddAsync(CourseSectionEntity section, CancellationToken ct = default)
        => await context.CourseSections.AddAsync(section, ct);

    /// <summary>Returns the tracked section, or null if not found.</summary>
    /// <param name="id">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<CourseSectionEntity?> GetByIdAsync(long id, CancellationToken ct = default)
        => context.CourseSections.FirstOrDefaultAsync(x => x.Id == CourseSectionId.From(id), ct);

    /// <summary>Stages an existing section for update; persisted on SaveAsync.</summary>
    /// <param name="section">The tracked section to update.</param>
    public void Update(CourseSectionEntity section)
        => context.Entry(section).State = EntityState.Modified;

    /// <summary>Marks the section as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="section">The tracked section to soft-delete.</param>
    public void SoftDelete(CourseSectionEntity section) => section.MarkDeleted();

    /// <summary>Checks whether the section still has a live episode.</summary>
    /// <param name="sectionId">Section identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<bool> HasEpisodesAsync(long sectionId, CancellationToken ct = default)
    {
        var id = CourseSectionId.From(sectionId);
        return context.Episodes.AnyAsync(x => x.SectionId == id, ct);
    }

    /// <summary>Stages the new display-order sequence for one course; persisted on SaveAsync.</summary>
    /// <param name="courseId">The course whose curriculum is reordered.</param>
    /// <param name="orderedIds">The section ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ReorderAsync(
        long courseId,
        IReadOnlyList<long> orderedIds,
        CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var course = CourseId.From(courseId);
        var ids = orderedIds.Select(CourseSectionId.From).ToList();

        // Load only the affected, live rows; unknown ids are silently skipped.
        var rows = await context.CourseSections
            .Where(x => x.CourseId == course && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < ids.Count; i++)
            if (rows.TryGetValue(ids[i], out var section))
                section.SetDisplayOrder(i + 1);
    }

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence for one course (global query
    /// filter excludes soft-deleted rows); persisted on SaveAsync.
    /// </summary>
    /// <param name="courseId">The course whose curriculum is renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RenumberAsync(long courseId, CancellationToken ct = default)
    {
        var course = CourseId.From(courseId);

        var rows = await context.CourseSections
            .Where(x => x.CourseId == course)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < rows.Count; i++)
            rows[i].SetDisplayOrder(i + 1);
    }
}
