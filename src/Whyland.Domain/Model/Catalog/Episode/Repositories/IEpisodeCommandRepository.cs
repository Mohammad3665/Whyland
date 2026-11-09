using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;

namespace Whyland.Domain.Model.Catalog.Episode.Repositories;

/// <summary>
/// Command-side repository for the <see cref="EpisodeEntity"/> aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IEpisodeCommandRepository
{
    /// <summary>Returns the tracked entity, or null if not found.</summary>
    /// <param name="id">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked episode, or null if not found.</returns>
    Task<EpisodeEntity?> GetByIdAsync(EpisodeId id, CancellationToken ct = default);

    /// <summary>Stages a new entity; persisted on SaveAsync.</summary>
    /// <param name="episode">The episode to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(EpisodeEntity episode, CancellationToken ct = default);

    /// <summary>Stages an existing entity for update; persisted on SaveAsync.</summary>
    /// <param name="episode">The tracked episode to update.</param>
    void Update(EpisodeEntity episode);

    /// <summary>Marks the entity as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="episode">The tracked episode to soft-delete.</param>
    void SoftDelete(EpisodeEntity episode);

    /// <summary>Checks whether the episode still has a live attachment.</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True when at least one live attachment belongs to the episode.</returns>
    Task<bool> HasAttachmentsAsync(EpisodeId episodeId, CancellationToken ct = default);

    /// <summary>Stages the new display-order sequence for one section.</summary>
    /// <param name="sectionId">The section whose episodes are reordered.</param>
    /// <param name="orderedIds">The episode ids in the desired order (index + 1 becomes DisplayOrder).</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReorderAsync(CourseSectionId sectionId, IReadOnlyList<EpisodeId> orderedIds, CancellationToken ct = default);

    /// <summary>Stages a compaction of DisplayOrder to 1..n for one section.</summary>
    /// <param name="sectionId">The section whose episodes are renumbered.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RenumberAsync(CourseSectionId sectionId, CancellationToken ct = default);
}
