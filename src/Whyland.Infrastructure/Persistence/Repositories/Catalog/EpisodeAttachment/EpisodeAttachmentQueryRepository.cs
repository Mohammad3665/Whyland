using EpisodeAttachmentEntity = Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities.EpisodeAttachment;
using Whyland.Infrastructure.Persistence.Queries;
using Whyland.Shared.Dtos.Catalog.Episode;
using Whyland.Domain.Model.Catalog.EpisodeAttachment.Repositories;

namespace Whyland.Infrastructure.Persistence.Repositories.Catalog.EpisodeAttachment;

/// <summary>
/// Query-side repository for the <see cref="EpisodeAttachment"/> aggregate. Implemented with the
/// shared Dapper query builder; the storage path is only ever read by the download feature, never
/// by the admin list (s8.2.1).
/// </summary>
/// <param name="dataSource">
/// The <see cref="NpgsqlDataSource"/> used to open pooled connections to the PostgreSQL database.
/// </param>
public sealed class EpisodeAttachmentQueryRepository(NpgsqlDataSource dataSource) : IEpisodeAttachmentQueryRepository
{
    private const string catalog = SchemaConstants.Catalog;

    /// <summary>
    /// Returns every live attachment of one episode, oldest first, so the admin list matches the
    /// upload order and two rows sharing a timestamp still come back in a stable order.
    /// </summary>
    /// <param name="episodeId">The episode whose attachments are read.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<IReadOnlyList<EpisodeAttachmentListItemDto>> GetForEpisodeAsync(
        EpisodeId episodeId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<EpisodeAttachmentEntity>.Create(
                $"{catalog}.EpisodeAttachments",
                x => x.Id,
                x => x.EpisodeId,
                x => x.FileName,
                x => x.ContentType,
                x => x.Size,
                x => x.CreatedAt)
            .Where(x => x.EpisodeId, episodeId.Value)
            .OrderBy(x => x.CreatedAt)
            .TieBreakBy(x => x.Id)
            .GetAllAsync<EpisodeAttachmentListItemDto>(dataSource, ct);

    /// <summary>
    /// Resolves the file metadata the download endpoint needs, or null when the attachment is gone.
    /// </summary>
    /// <param name="attachmentId">Attachment identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<EpisodeAttachmentDownloadDto?> GetDownloadInfoAsync(
        EpisodeAttachmentId attachmentId,
        CancellationToken ct = default)
        => DapperFluentQueryBuilder<EpisodeAttachmentEntity>.Create(
                $"{catalog}.EpisodeAttachments",
                x => x.Id,
                x => x.EpisodeId,
                x => x.FileName,
                x => x.ContentType,
                x => x.StoragePath,
                x => x.Size)
            .Where(x => x.Id, attachmentId.Value)
            .GetFirstOrDefaultAsync<EpisodeAttachmentDownloadDto>(dataSource, ct);
}
