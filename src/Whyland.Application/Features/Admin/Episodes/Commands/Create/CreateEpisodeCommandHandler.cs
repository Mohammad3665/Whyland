namespace Whyland.Application.Features.Admin.Episodes.Commands.Create;

/// <summary>
/// Handles <see cref="CreateEpisodeCommand"/> by confirming the parent section, creating the episode
/// through the domain factory, persisting it via the unit of work, and compacting the section's
/// episode order to 1..n.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to section and episode persistence.</param>
public sealed class CreateEpisodeCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEpisodeCommand, EpisodeId>
{
    /// <summary>
    /// Creates a new episode through the domain factory, or returns an error when the section does
    /// not exist, the factory rejects the input or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new episode values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created episode, or an error describing the failure.</returns>
    public async Task<Result<EpisodeId>> Handle(CreateEpisodeCommand command, CancellationToken ct)
    {
        var section = await unitOfWork.CourseSectionCommandRepository.GetByIdAsync(command.SectionId, ct);
        if (section is null)
            return CourseSectionErrors.NotFound(command.SectionId);

        var created = Episode.Create(
            section.Id,
            command.Title,
            command.Description,
            command.DisplayOrder,
            command.IsFree);

        if (created.IsFailure)
            return created.Error;

        var episode = created.Data!;

        await unitOfWork.EpisodeCommandRepository.AddAsync(episode, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return EpisodeErrors.CreateFailed();

        // Compact 1..n so a client-entered order (10, 17, 100, ...) still yields a clean sequence
        // matching each episode's sorted position.
        await unitOfWork.EpisodeCommandRepository.RenumberAsync(command.SectionId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? EpisodeErrors.CreateFailed() : episode.Id;
    }
}
