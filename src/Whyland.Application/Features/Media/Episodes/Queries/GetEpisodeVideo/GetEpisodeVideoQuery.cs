namespace Whyland.Application.Features.Media.Episodes.Queries.GetEpisodeVideo;

/// <summary>
/// Query that authorizes one playback request and resolves its video source: a protected file this
/// platform streams, or an external link the caller may follow (s8.1.3, s8.1.5).
/// </summary>
/// <param name="EpisodeId">The episode whose video is requested.</param>
/// <param name="UserId">The signed-in user, or null for an anonymous caller.</param>
/// <param name="IsStaff">Whether the caller holds the course-management permission.</param>
public sealed record GetEpisodeVideoQuery(
    EpisodeId EpisodeId,
    UserId? UserId,
    bool IsStaff) : IQuery<EpisodeVideoSource>;
