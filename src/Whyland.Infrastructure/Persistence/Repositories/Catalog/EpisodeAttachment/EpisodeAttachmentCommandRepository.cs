using Whyland.Domain.Model.Catalog.EpisodeAttachment.Repositories;
using EpisodeAttachmentEntity = Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities.EpisodeAttachment;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.EpisodeAttachment;

/// <summary>
/// The EF Core command repository for the <c>EpisodeAttachment</c> aggregate, backed by the shared
/// scoped <see cref="WhylandDbContext"/>; persistence happens through the Unit of Work.
/// </summary>
/// <param name="context">The scoped EF Core context the repository stages changes on.</param>
public sealed class EpisodeAttachmentCommandRepository(WhylandDbContext context) : IEpisodeAttachmentCommandRepository
{
    /// <summary>Stages a new attachment; persisted on SaveAsync.</summary>
    /// <param name="attachment">The attachment to add.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task AddAsync(EpisodeAttachmentEntity attachment, CancellationToken ct = default)
        => await context.EpisodeAttachments.AddAsync(attachment, ct);

    /// <summary>Returns the tracked attachment, or null if not found.</summary>
    /// <param name="id">Attachment identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeAttachmentEntity?> GetByIdAsync(EpisodeAttachmentId id, CancellationToken ct = default)
        => context.EpisodeAttachments.FirstOrDefaultAsync(x => x.Id == id, ct);

    /// <summary>Stages an existing attachment for update; persisted on SaveAsync.</summary>
    /// <param name="attachment">The tracked attachment to update.</param>
    public void Update(EpisodeAttachmentEntity attachment)
        => context.Entry(attachment).State = EntityState.Modified;

    /// <summary>Marks the attachment as soft-deleted (hidden by the global query filter).</summary>
    /// <param name="attachment">The tracked attachment to soft-delete.</param>
    public void SoftDelete(EpisodeAttachmentEntity attachment) => attachment.MarkDeleted();

    /// <summary>Counts the live attachments of one episode, against the configured maximum (s7.8).</summary>
    /// <param name="episodeId">Episode identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<int> CountForEpisodeAsync(EpisodeId episodeId, CancellationToken ct = default)
        => context.EpisodeAttachments.CountAsync(x => x.EpisodeId == episodeId, ct);
}
