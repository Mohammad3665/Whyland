namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>Creates a section, persists it and compacts the course's section order to 1..n.</summary>
public sealed class CreateCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCourseSectionCommand, long>
{
    public async Task<Result<long>> Handle(CreateCourseSectionCommand command, CancellationToken ct)
    {
        var course = await unitOfWork.CourseCommandRepository.GetByIdAsync(command.CourseId, ct);
        if (course is null)
            return CourseErrors.NotFound(command.CourseId);

        var created = CourseSection.Create(course.Id, command.Title, command.DisplayOrder);
        if (created.IsFailure)
            return created.Error;

        var section = created.Data!;

        await unitOfWork.CourseSectionCommandRepository.AddAsync(section, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseSectionErrors.CreateFailed();

        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(command.CourseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.CreateFailed() : section.Id.Value;
    }
}
