namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteCourseSectionCommand"/> by soft-deleting an empty section through the
/// command repository and compacting the course order via the unit of work. A section with episodes
/// is refused: soft-deleting it would hide the episodes without deleting them.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to section persistence.</param>
public sealed class DeleteCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourseSectionCommand>
{
    /// <summary>
    /// Soft-deletes the requested section, or returns an error when it does not exist, still has
    /// episodes, or persistence fails.
    /// </summary>
    /// <param name="command">The command containing the section identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteCourseSectionCommand command, CancellationToken ct)
    {
        var section = await unitOfWork.CourseSectionCommandRepository.GetByIdAsync(command.Id, ct);
        if (section is null)
            return CourseSectionErrors.NotFound(command.Id);

        if (await unitOfWork.CourseSectionCommandRepository.HasEpisodesAsync(command.Id, ct))
            return CourseSectionErrors.HasEpisodes(command.Id);

        var courseId = section.CourseId.Value;

        unitOfWork.CourseSectionCommandRepository.SoftDelete(section);

        // First save persists the soft delete so the compaction below does not
        // include (and renumber around) the deleted row.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseSectionErrors.SoftDeleteFailed();

        // Close any DisplayOrder gap left behind, then persist the new numbering.
        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(courseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.SoftDeleteFailed() : Result.Success();
    }
}
