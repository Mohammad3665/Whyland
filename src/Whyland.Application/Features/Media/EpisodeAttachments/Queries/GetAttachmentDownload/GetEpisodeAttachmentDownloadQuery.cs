namespace Whyland.Application.Features.Media.EpisodeAttachments.Queries.GetAttachmentDownload;

/// <summary>
/// Query that authorizes one attachment download and opens the protected file (s8.2.1).
/// </summary>
/// <param name="AttachmentId">The attachment whose file is requested.</param>
/// <param name="UserId">The signed-in user, or null for an anonymous caller.</param>
/// <param name="IsStaff">Whether the caller holds the course-management permission.</param>
public sealed record GetEpisodeAttachmentDownloadQuery(
    EpisodeAttachmentId AttachmentId,
    UserId? UserId,
    bool IsStaff) : IQuery<EpisodeAttachmentFile>;
