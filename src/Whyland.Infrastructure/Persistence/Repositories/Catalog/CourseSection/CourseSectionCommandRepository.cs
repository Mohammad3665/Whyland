using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseSection;

/// <summary>
/// EF Core command repository for <see cref="CourseSectionEntity"/>. Ordering is always scoped to
/// one course, so reordering a section never renumbers another course's curriculum.
/// </summary>
public sealed class CourseSectionCommandRepository(WhylandDbContext context) : ICourseSectionCommandRepository
{
    public async Task AddAsync(CourseSectionEntity section, CancellationToken ct = default)
        => await context.CourseSections.AddAsync(section, ct);

    public Task<CourseSectionEntity?> GetByIdAsync(long id, CancellationToken ct = default)
        => context.CourseSections.FirstOrDefaultAsync(x => x.Id == CourseSectionId.From(id), ct);

    public void Update(CourseSectionEntity section)
        => context.Entry(section).State = EntityState.Modified;

    public void SoftDelete(CourseSectionEntity section) => section.MarkDeleted();

    public Task<bool> HasEpisodesAsync(long sectionId, CancellationToken ct = default)
    {
        var id = CourseSectionId.From(sectionId);
        return context.Episodes.AnyAsync(x => x.SectionId == id, ct);
    }

    public async Task ReorderAsync(
        long courseId,
        IReadOnlyList<long> orderedIds,
        CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var course = CourseId.From(courseId);
        var ids = orderedIds.Select(CourseSectionId.From).ToList();

        var rows = await context.CourseSections
            .Where(x => x.CourseId == course && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < ids.Count; i++)
            if (rows.TryGetValue(ids[i], out var section))
                section.SetDisplayOrder(i + 1);
    }

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
