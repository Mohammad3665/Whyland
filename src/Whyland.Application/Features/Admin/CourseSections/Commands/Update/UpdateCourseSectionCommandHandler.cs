namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateCourseSectionCommand"/> by loading the section, applying the domain
/// <c>Update</c> method, persisting the change via the unit of work, and compacting the order of
/// its course.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to section persistence.</param>
public sealed class UpdateCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCourseSectionCommand>
{
    /// <summary>
    /// Updates the requested section, or returns an error when it does not exist, the domain update
    /// is rejected, or persistence fails.
    /// </summary>
    /// <param name="command">The command with the updated section values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(UpdateCourseSectionCommand command, CancellationToken ct)
    {
        var section = await unitOfWork.CourseSectionCommandRepository.GetByIdAsync(command.Id, ct);
        if (section is null)
            return CourseSectionErrors.NotFound(command.Id);

        var updated = section.Update(command.Title, command.DisplayOrder);
        if (updated.IsFailure)
            return updated.Error;

        unitOfWork.CourseSectionCommandRepository.Update(section);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseSectionErrors.UpdateFailed();

        // Compact 1..n so a client-entered order (10, 17, 100, ...) still
        // yields a clean sequence matching each section's sorted position.
        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(section.CourseId.Value, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.UpdateFailed() : Result.Success();
    }
}
