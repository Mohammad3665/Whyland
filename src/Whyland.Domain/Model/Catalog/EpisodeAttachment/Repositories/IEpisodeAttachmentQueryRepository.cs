using Whyland.Shared.Dtos.Catalog.Episode;

namespace Whyland.Domain.Model.Catalog.EpisodeAttachment.Repositories;

/// <summary>
/// Query-side repository for the <see cref="EpisodeAttachment"/> aggregate.
/// Implemented with Dapper for lightweight, non-tracked read queries.
/// </summary>
public interface IEpisodeAttachmentQueryRepository
{
    /// <summary>
    /// Returns every live attachment of one episode, newest last, so the admin list matches the upload
    /// order.
    /// </summary>
    /// <param name="episodeId">The episode whose attachments are read.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of attachments.</returns>
    Task<IReadOnlyList<EpisodeAttachmentListItemDto>> GetForEpisodeAsync(
        EpisodeId episodeId,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves the file metadata the download endpoint needs, or null when the attachment is gone.
    /// </summary>
    /// <param name="attachmentId">Attachment identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The download metadata, or null.</returns>
    Task<EpisodeAttachmentDownloadDto?> GetDownloadInfoAsync(
        EpisodeAttachmentId attachmentId,
        CancellationToken ct = default);
}
