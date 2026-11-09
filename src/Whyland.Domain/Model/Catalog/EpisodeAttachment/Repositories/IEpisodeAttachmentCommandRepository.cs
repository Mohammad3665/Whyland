using EpisodeAttachmentEntity = Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities.EpisodeAttachment;

namespace Whyland.Domain.Model.Catalog.EpisodeAttachment.Repositories;

/// <summary>
/// Command-side repository for the <see cref="EpisodeAttachmentEntity"/> aggregate.
/// Implemented with EF Core (change tracking); persistence goes through
/// <c>IUnitOfWork.SaveAsync()</c> — this interface never saves on its own.
/// </summary>
public interface IEpisodeAttachmentCommandRepository
{
    /// <summary>Returns the tracked attachment, or null if not found.</summary>
    /// <param name="id">Attachment identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tracked attachment, or null if not found.</returns>
    Task<EpisodeAttachmentEntity?> GetByIdAsync(EpisodeAttachmentId id, CancellationToken ct = default);

    /// <summary>Stages a new attachment; persisted on SaveAsync.</summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(EpisodeAttachmentEntity attachment, CancellationToken ct = default);

    /// <summary>Stages an existing attachment for update; persisted on SaveAsync.</summary>
    /// <param name="attachment">The tracked attachment to update.</param>
    void Update(EpisodeAttachmentEntity attachment);

    /// <summary>Marks the attachment as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="attachment">The tracked attachment to soft-delete.</param>
    void SoftDelete(EpisodeAttachmentEntity attachment);

    /// <summary>Counts the live attachments of one episode, against the configured maximum (s7.8).</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>How many live attachments the episode currently holds.</returns>
    Task<int> CountForEpisodeAsync(EpisodeId episodeId, CancellationToken ct = default);
}
