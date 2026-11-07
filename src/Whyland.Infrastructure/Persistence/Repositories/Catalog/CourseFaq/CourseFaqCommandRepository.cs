using CourseFaqEntity = Whyland.Domain.Model.Catalog.CourseFaq.Entities.CourseFaq;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.CourseFaq;

/// <summary>
/// EF Core command repository for <see cref="CourseFaqEntity"/>. Ordering is always scoped to one
/// course, so reordering a question never renumbers another course's FAQ list.
/// </summary>
public sealed class CourseFaqCommandRepository(WhylandDbContext context) : ICourseFaqCommandRepository
{
    public async Task AddAsync(CourseFaqEntity faq, CancellationToken ct = default)
        => await context.CourseFaqs.AddAsync(faq, ct);

    public Task<CourseFaqEntity?> GetByIdAsync(long id, CancellationToken ct = default)
        => context.CourseFaqs.FirstOrDefaultAsync(x => x.Id == CourseFaqId.From(id), ct);

    public void Update(CourseFaqEntity faq)
        => context.Entry(faq).State = EntityState.Modified;

    public void SoftDelete(CourseFaqEntity faq) => faq.MarkDeleted();

    public async Task ReorderAsync(
        long courseId,
        IReadOnlyList<long> orderedIds,
        CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var course = CourseId.From(courseId);
        var ids = orderedIds.Select(CourseFaqId.From).ToList();

        var rows = await context.CourseFaqs
            .Where(x => x.CourseId == course && ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < ids.Count; i++)
            if (rows.TryGetValue(ids[i], out var faq))
                faq.SetDisplayOrder(i + 1);
    }

    public async Task RenumberAsync(long courseId, CancellationToken ct = default)
    {
        var course = CourseId.From(courseId);

        var rows = await context.CourseFaqs
            .Where(x => x.CourseId == course)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < rows.Count; i++)
            rows[i].SetDisplayOrder(i + 1);
    }
}
