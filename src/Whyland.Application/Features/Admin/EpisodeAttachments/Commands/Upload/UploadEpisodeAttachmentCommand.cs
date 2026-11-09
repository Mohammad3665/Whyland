namespace Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Upload;

/// <summary>
/// Command that validates and stores one downloadable attachment for an episode (s7.8, s8.3.1).
/// </summary>
/// <param name="EpisodeId">The episode that receives the file.</param>
/// <param name="Attachment">The uploaded file; the stream stays owned by the caller.</param>
public sealed record UploadEpisodeAttachmentCommand(
    EpisodeId EpisodeId,
    EpisodeAttachmentUpload Attachment) : ICommand<EpisodeAttachmentId>;
