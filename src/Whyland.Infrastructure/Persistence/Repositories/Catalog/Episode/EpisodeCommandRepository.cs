using Whyland.Domain.Model.Catalog.Episode.Repositories;
using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.Episode;

/// <summary>
/// The EF Core command repository for the <c>Episode</c> aggregate, backed by the shared scoped
/// <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work. Ordering is always
/// scoped to one section, so reordering an episode never renumbers another section's playlist.
/// </summary>
/// <param name="context">The scoped EF Core context the repository stages changes on.</param>
public sealed class EpisodeCommandRepository(WhylandDbContext context) : IEpisodeCommandRepository
{
    /// <summary>Stages a new episode; persisted on SaveAsync.</summary>
    /// <param name="episode">The episode to add.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task AddAsync(EpisodeEntity episode, CancellationToken ct = default)
        => await context.Episodes.AddAsync(episode, ct);

    /// <summary>Returns the tracked episode, or null if not found.</summary>
    /// <param name="id">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeEntity?> GetByIdAsync(EpisodeId id, CancellationToken ct = default)
        => context.Episodes.FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <summary>Stages an existing episode for update; persisted on SaveAsync.</summary>
    /// <param name="episode">The tracked episode to update.</param>
    public void Update(EpisodeEntity episode)
        => context.Entry(episode).State = EntityState.Modified;

    /// <summary>Marks the episode as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="episode">The tracked episode to soft-delete.</param>
    public void SoftDelete(EpisodeEntity episode) => episode.MarkDeleted();

    /// <summary>Checks whether the episode still has a live attachment.</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<bool> HasAttachmentsAsync(EpisodeId episodeId, CancellationToken ct = default)
        => context.EpisodeAttachments.AnyAsync(x => x.EpisodeId == episodeId, ct);

    /// <summary>
    /// Stages the new display-order sequence for one section. Only the affected, live rows are
    /// loaded; unknown ids are silently skipped.
    /// </summary>
    /// <param name="sectionId">The section whose episodes are reordered.</param>
    /// <param name="orderedIds">The episode ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ReorderAsync(
        CourseSectionId sectionId,
        IReadOnlyList<EpisodeId> orderedIds,
        CancellationToken ct = default)
    {
        if (orderedIds.Count == 0)
            return;

        var rows = await context.Episodes
            .Where(x => x.SectionId == sectionId && orderedIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, ct);

        for (var i = 0; i < orderedIds.Count; i++)
            if (rows.TryGetValue(orderedIds[i], out var episode))
                episode.SetDisplayOrder(i + 1);
    }

    /// <summary>
    /// Stages a compaction of DisplayOrder to an unbroken 1..n sequence for one section (global
    /// query filter excludes soft-deleted rows); persisted on SaveAsync.
    /// </summary>
    /// <param name="sectionId">The section whose episodes are renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RenumberAsync(CourseSectionId sectionId, CancellationToken ct = default)
    {
        var rows = await context.Episodes
            .Where(x => x.SectionId == sectionId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(ct);

        for (var i = 0; i < rows.Count; i++)
            rows[i].SetDisplayOrder(i + 1);
    }
}
