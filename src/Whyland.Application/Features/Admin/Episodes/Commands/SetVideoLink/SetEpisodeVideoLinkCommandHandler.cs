using Whyland.Application.Common.Abstractions.Storage;
using Whyland.Application.Common.Abstractions.Video;

namespace Whyland.Application.Features.Admin.Episodes.Commands.SetVideoLink;

/// <summary>
/// Handles <see cref="SetEpisodeVideoLinkCommand"/> by validating the link on the aggregate,
/// persisting it, and removing a previously uploaded file once the new link is safely saved.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
/// <param name="videoStorage">The protected storage that owns previously uploaded files.</param>
/// <param name="videoMetadata">Reads the duration from a supported external provider.</param>
/// <param name="logger">The logger used to record a file that could not be removed.</param>
public sealed class SetEpisodeVideoLinkCommandHandler(
    IUnitOfWork unitOfWork,
    IVideoStorageService videoStorage,
    IExternalVideoMetadataService videoMetadata,
    ILogger<SetEpisodeVideoLinkCommandHandler> logger)
    : ICommandHandler<SetEpisodeVideoLinkCommand>
{
    /// <summary>
    /// Sets the external link, or returns a validation or persistence error. Replacing an uploaded
    /// file deletes that file only after the database has accepted the new link.
    /// </summary>
    /// <param name="command">The episode identifier and the external link.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(SetEpisodeVideoLinkCommand command, CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.Id, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.Id.Value);

        var previousKey = episode.Video;

        // The link stays valid even when the provider cannot report a duration; the table then
        // keeps its dash instead of blocking the save.
        var duration = await videoMetadata.TryGetDurationAsync(command.Url, ct);

        var linked = episode.SetVideoUrl(command.Url, duration);
        if (linked.IsFailure)
            return linked.Error;

        unitOfWork.EpisodeCommandRepository.Update(episode);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeErrors.UpdateFailed();

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
