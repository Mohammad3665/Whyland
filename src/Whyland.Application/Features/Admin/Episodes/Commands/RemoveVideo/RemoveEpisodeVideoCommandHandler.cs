using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.Episodes.Commands.RemoveVideo;

/// <summary>
/// Handles <see cref="RemoveEpisodeVideoCommand"/> by clearing the video key and duration on the
/// episode, persisting that first, and only then deleting the file: a database failure must never
/// leave a row pointing at a file that is already gone.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
/// <param name="videoStorage">The protected storage that owns the video file.</param>
/// <param name="logger">The logger used to record a file that could not be removed.</param>
public sealed class RemoveEpisodeVideoCommandHandler(
    IUnitOfWork unitOfWork,
    IVideoStorageService videoStorage,
    ILogger<RemoveEpisodeVideoCommandHandler> logger)
    : ICommandHandler<RemoveEpisodeVideoCommand>
{
    /// <summary>
    /// Clears the video of the requested episode and deletes its file, or returns an error when the
    /// episode does not exist or persistence fails.
    /// </summary>
    /// <param name="command">The command containing the episode identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(RemoveEpisodeVideoCommand command, CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.Id, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.Id.Value);

        var videoKey = episode.Video;
        if (string.IsNullOrWhiteSpace(videoKey))
            return Result.Success();

        episode.ClearVideo();
        unitOfWork.EpisodeCommandRepository.Update(episode);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeErrors.UpdateFailed();

        // Nothing to delete on disk when the episode pointed at an external host.
        if (Episode.IsExternalVideo(videoKey))
            return Result.Success();

        var deleted = await videoStorage.DeleteAsync(videoKey, ct);
        if (deleted.IsFailure)
            logger.LogWarning(
                "The removed video file {VideoKey} of episode {EpisodeId} could not be deleted.",
                videoKey,
                episode.Id.Value);

        return Result.Success();
    }
}
