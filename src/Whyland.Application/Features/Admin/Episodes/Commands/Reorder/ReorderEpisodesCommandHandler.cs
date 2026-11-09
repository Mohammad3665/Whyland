namespace Whyland.Application.Features.Admin.Episodes.Commands.Reorder;

/// <summary>
/// Handles <see cref="ReorderEpisodesCommand"/> by renumbering DisplayOrder 1..n in the given visual
/// order and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to episode persistence.</param>
public sealed class ReorderEpisodesCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderEpisodesCommand>
{
    /// <summary>
    /// Persists the new display order, or returns an error when persistence fails.
    /// </summary>
    /// <param name="command">The command carrying the new ordering sequence.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(ReorderEpisodesCommand command, CancellationToken ct)
    {
        await unitOfWork.EpisodeCommandRepository.ReorderAsync(
            command.SectionId,
            command.OrderedIds,
            ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? EpisodeErrors.ReorderFailed() : Result.Success();
    }
}
