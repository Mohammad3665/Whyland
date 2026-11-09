namespace Whyland.Application.Features.Admin.Episodes.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateEpisodeCommand"/> by loading the episode, applying the domain
/// <c>Update</c> method, persisting the change via the unit of work, and compacting the order of its
/// section.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
public sealed class UpdateEpisodeCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEpisodeCommand>
{
    /// <summary>
    /// Updates the requested episode, or returns an error when it does not exist, the domain update
    /// is rejected, or persistence fails.
    /// </summary>
    /// <param name="command">The command with the updated episode values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateEpisodeCommand command, CancellationToken ct)
    {
        var episode = await unitOfWork.EpisodeCommandRepository.GetByIdAsync(command.Id, ct);
        if (episode is null)
            return EpisodeErrors.NotFound(command.Id.Value);

        var updated = episode.Update(
            command.Title,
            command.Description,
            command.DisplayOrder,
            command.IsFree);

        if (updated.IsFailure)
            return updated.Error;

        unitOfWork.EpisodeCommandRepository.Update(episode);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeErrors.UpdateFailed();

        await unitOfWork.EpisodeCommandRepository.RenumberAsync(episode.SectionId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? EpisodeErrors.UpdateFailed() : Result.Success();
    }
}
