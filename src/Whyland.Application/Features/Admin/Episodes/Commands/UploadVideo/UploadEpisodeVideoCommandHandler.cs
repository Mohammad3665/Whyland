using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.Episodes.Commands.UploadVideo;

/// <summary>
/// Handles <see cref="UploadEpisodeVideoCommand"/> by storing the file in protected storage, linking
/// the server-generated key and the probed duration to the episode, and removing the previous file
/// once the new one is safely persisted.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
/// <param name="videoStorage">The protected storage that validates and owns the video file.</param>
/// <param name="logger">The logger used to record a previous file that could not be removed.</param>
public sealed class UploadEpisodeVideoCommandHandler(
    IUnitOfWork unitOfWork,
    IVideoStorageService videoStorage,
    ILogger<UploadEpisodeVideoCommandHandler> logger)
    : ICommandHandler<UploadEpisodeVideoCommand>
{
    /// <summary>
    /// Stores and attaches the video, or returns the storage rejection, the domain error or a
    /// persistence failure. A file written before a later step fails is removed again, so a failed
    /// save never leaves an orphan behind.
    /// </summary>
    /// <param name="command">The episode identifier and the uploaded file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UploadEpisodeVideoCommand command, CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.Id, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.Id.Value);

        var saved = await videoStorage.SaveAsync(
            command.Video.Content,
            command.Video.FileName,
            command.Video.ContentType,
            command.Video.Length,
            ct);

        if (saved.IsFailure)
            return saved.Error;

        var stored = saved.Data!;
        var previousKey = episode.Video;

        var linked = episode.SetVideo(stored.RelativePath, stored.Duration);
        if (linked.IsFailure)
        {
            await videoStorage.DeleteAsync(stored.RelativePath, ct);
            return linked.Error;
        }

        unitOfWork.EpisodeCommandRepository.Update(episode);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
        {
            await videoStorage.DeleteAsync(stored.RelativePath, ct);
            return EpisodeErrors.VideoUploadFailed();
        }

        // Only an uploaded file is ours to delete; replacing an external link must not touch the host.
        if (!string.IsNullOrWhiteSpace(previousKey) && !Episode.IsExternalVideo(previousKey))
        {
            var deleted = await videoStorage.DeleteAsync(previousKey, ct);
            if (deleted.IsFailure)
                logger.LogWarning(
                    "The replaced video file {VideoKey} of episode {EpisodeId} could not be removed.",
                    previousKey,
                    episode.Id.Value);
        }

        return Result.Success();
    }
}
