namespace Whyland.Application.Features.Admin.Episodes.Commands.UploadVideo;

/// <summary>
/// Command that stores a validated MP4 as the protected video of one episode and records the
/// duration the container reports (s7.7.1, s8.3.2).
/// </summary>
/// <param name="Id">The episode that receives the video.</param>
/// <param name="Video">The uploaded file; the stream stays owned by the caller.</param>
public sealed record UploadEpisodeVideoCommand(EpisodeId Id, EpisodeVideoUpload Video) : ICommand;
