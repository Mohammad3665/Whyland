namespace Whyland.Application.Features.Admin.EpisodeAttachments.Queries.GetForEpisode;

/// <summary>
/// Query that returns the live attachments of one episode in upload order.
/// </summary>
/// <param name="EpisodeId">The episode whose attachments are read.</param>
public sealed record GetEpisodeAttachmentsQuery(EpisodeId EpisodeId)
    : IQuery<IReadOnlyList<EpisodeAttachmentListItemDto>>;
