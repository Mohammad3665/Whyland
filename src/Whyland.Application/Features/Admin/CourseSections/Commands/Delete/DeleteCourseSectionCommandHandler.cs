namespace Whyland.Application.Features.Admin.CourseSections.Commands.Delete;

/// <summary>
/// Soft-deletes an empty section and compacts the course order. A section with episodes is refused:
/// soft-deleting it would hide the episodes without deleting them.
/// </summary>
public sealed class DeleteCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourseSectionCommand>
{
    public async Task<Result> Handle(DeleteCourseSectionCommand command, CancellationToken ct)
    {
        var section = await unitOfWork.CourseSectionCommandRepository.GetByIdAsync(command.Id, ct);
        if (section is null)
            return CourseSectionErrors.NotFound(command.Id);

        if (await unitOfWork.CourseSectionCommandRepository.HasEpisodesAsync(command.Id, ct))
            return CourseSectionErrors.HasEpisodes(command.Id);

        var courseId = section.CourseId.Value;

        unitOfWork.CourseSectionCommandRepository.SoftDelete(section);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseSectionErrors.SoftDeleteFailed();

        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(courseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.SoftDeleteFailed() : Result.Success();
    }
}
