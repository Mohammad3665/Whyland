namespace Whyland.Application.Features.Admin.CourseSections.Commands.Update;

/// <summary>Updates a section and compacts the order of its course.</summary>
public sealed class UpdateCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCourseSectionCommand>
{
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

        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(section.CourseId.Value, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.UpdateFailed() : Result.Success();
    }
}
