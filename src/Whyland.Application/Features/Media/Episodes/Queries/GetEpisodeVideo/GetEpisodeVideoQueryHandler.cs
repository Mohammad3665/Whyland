using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Media.Episodes.Queries.GetEpisodeVideo;

/// <summary>
/// Handles <see cref="GetEpisodeVideoQuery"/> by resolving the episode and its course, deciding
/// access before touching the disk, and only then returning the video source (s7.7.3, s8.1.3).
/// <para>
/// Access is: staff with the course-management permission; a signed-in owner or instructor of a
/// published course; or anyone for a free preview of a published course. A hidden or draft course
/// therefore never leaks through a free preview. An external link is returned only after the same
/// check, so the platform never becomes an open redirect.
/// </para>
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode query data.</param>
/// <param name="videoStorage">The protected storage that owns uploaded video files.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetEpisodeVideoQueryHandler(
    IUnitOfWork unitOfWork,
    IVideoStorageService videoStorage,
    ILogger<GetEpisodeVideoQueryHandler> logger)
    : IQueryHandler<GetEpisodeVideoQuery, EpisodeVideoSource>
{
    /// <summary>
    /// Returns the authorized video source, or an error when the episode does not exist, the caller
    /// may not watch it, or a protected file cannot be opened.
    /// </summary>
    /// <param name="query">The episode identifier and the caller's access inputs.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The authorized source, or an error describing the failure.</returns>
    public async Task<Result<EpisodeVideoSource>> Handle(
        GetEpisodeVideoQuery query,
        CancellationToken ct)
    {
        var info = await unitOfWork.EpisodeQueryRepository.GetPlaybackInfoAsync(query.EpisodeId, ct);
        if (info is null)
            return EpisodeErrors.NotFound(query.EpisodeId.Value);

        if (string.IsNullOrWhiteSpace(info.Video))
            return EpisodeErrors.VideoRequired();

        if (!query.IsStaff)
        {
            if (info.CourseDisplayStatus != DisplayStatus.Published)
                return EpisodeErrors.AccessDenied();

            var entitled = query.UserId is { } userId
                && await unitOfWork.EpisodeQueryRepository.HasCourseAccessAsync(
                    CourseId.From(info.CourseId),
                    userId,
                    ct);

            if (!entitled && !info.IsFree)
                return EpisodeErrors.AccessDenied();
        }

        // An external video is not stored or streamed here; the caller is sent to the host.
        if (Episode.IsExternalVideo(info.Video))
            return Result<EpisodeVideoSource>.Success(new EpisodeVideoSource(null, info.Video));

        var opened = await videoStorage.OpenAsync(info.Video, ct);
        if (opened.IsFailure)
        {
            logger.LogWarning(
                "The video file of episode {EpisodeId} could not be opened.",
                query.EpisodeId.Value);

            return Result<EpisodeVideoSource>.Failure(opened.Error);
        }

        return Result<EpisodeVideoSource>.Success(new EpisodeVideoSource(opened.Data, null));
    }
}
