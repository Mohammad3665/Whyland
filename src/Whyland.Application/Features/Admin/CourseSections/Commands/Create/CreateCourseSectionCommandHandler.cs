namespace Whyland.Application.Features.Admin.CourseSections.Commands.Create;

/// <summary>
/// Handles <see cref="CreateCourseSectionCommand"/> by confirming the parent course, creating the
/// section through the domain factory method, persisting it via the unit of work, and compacting
/// the course's section order to 1..n.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course and section persistence.</param>
public sealed class CreateCourseSectionCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCourseSectionCommand, CourseSectionId>
{
    /// <summary>
    /// Creates a new section through the domain factory, or returns an error when the course does
    /// not exist, the factory rejects the input or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new section values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created section, or an error describing the failure.</returns>
    public async Task<Result<CourseSectionId>> Handle(CreateCourseSectionCommand command, CancellationToken ct)
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

        // Compact 1..n: whatever order the client picked (10, 17, 100, ...),
        // the section lands on the sequence slot matching its sorted position.
        await unitOfWork.CourseSectionCommandRepository.RenumberAsync(command.CourseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.CreateFailed() : section.Id;
    }
}
