namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>
/// Handles <see cref="CreateCourseFaqCommand"/> by confirming the parent course, creating the
/// question through the domain factory method, persisting it via the unit of work, and compacting
/// the course's FAQ order to 1..n.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course and course-FAQ persistence.</param>
public sealed class CreateCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCourseFaqCommand, CourseFaqId>
{
    /// <summary>
    /// Creates a new course FAQ through the domain factory, or returns an error when the course does
    /// not exist, the factory rejects the input or persistence fails.
    /// </summary>
    /// <param name="command">The command with the new question values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The identifier of the created question, or an error describing the failure.</returns>
    public async Task<Result<CourseFaqId>> Handle(CreateCourseFaqCommand command, CancellationToken ct)
    {
        var course = await unitOfWork.CourseCommandRepository.GetByIdAsync(command.CourseId, ct);
        if (course is null)
            return CourseErrors.NotFound(command.CourseId);

        var created = CourseFaq.Create(
            course.Id,
            command.Title,
            command.Description,
            command.DisplayOrder,
            command.IsActive);

        if (created.IsFailure)
            return created.Error;

        var faq = created.Data!;

        await unitOfWork.CourseFaqCommandRepository.AddAsync(faq, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseFaqErrors.CreateFailed();

        // Compact 1..n: whatever order the client picked (10, 17, 100, ...),
        // the question lands on the sequence slot matching its sorted position.
        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(command.CourseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.CreateFailed() : faq.Id;
    }
}
