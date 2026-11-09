using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.Episodes.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteEpisodeCommand"/> by soft-deleting an episode that no longer carries
/// attachments, removing its protected video file and compacting the section order. An episode with
/// attachments is refused: deleting it would orphan the files and their metadata (s7.8).
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
/// <param name="videoStorage">The protected storage that owns the episode video file.</param>
/// <param name="logger">The logger used to record a file that could not be removed.</param>
public sealed class DeleteEpisodeCommandHandler(
    IUnitOfWork unitOfWork,
    IVideoStorageService videoStorage,
    ILogger<DeleteEpisodeCommandHandler> logger)
    : ICommandHandler<DeleteEpisodeCommand>
{
    /// <summary>
    /// Soft-deletes the requested episode, or returns an error when it does not exist, still has
    /// attachments, or persistence fails.
    /// </summary>
    /// <param name="command">The command containing the episode identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteEpisodeCommand command, CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.Id, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.Id.Value);

        if (await unitOfWork.EpisodeCommandRepository.HasAttachmentsAsync(command.Id, ct))
            return EpisodeErrors.HasAttachments(command.Id.Value);

        var sectionId = episode.SectionId;
        var videoKey = episode.Video;

        unitOfWork.EpisodeCommandRepository.SoftDelete(episode);

        // First save persists the soft delete so the compaction below does not include the hidden row.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeErrors.SoftDeleteFailed();

        // An external link is not a file this platform owns, so there is nothing to delete.
        if (!string.IsNullOrWhiteSpace(videoKey) && !Episode.IsExternalVideo(videoKey))
        {
            var deleted = await videoStorage.DeleteAsync(videoKey, ct);
            if (deleted.IsFailure)
                logger.LogWarning("The video file of deleted episode {EpisodeId} could not be removed.", episode.Id.Value);
        }

        await unitOfWork.EpisodeCommandRepository.RenumberAsync(sectionId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? EpisodeErrors.SoftDeleteFailed() : Result.Success();
    }
}
