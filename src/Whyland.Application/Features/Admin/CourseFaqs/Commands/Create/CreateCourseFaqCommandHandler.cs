namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;

/// <summary>Creates a course FAQ, persists it and compacts the course's FAQ order to 1..n.</summary>
public sealed class CreateCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCourseFaqCommand, long>
{
    public async Task<Result<long>> Handle(CreateCourseFaqCommand command, CancellationToken ct)
    {
        var course = await unitOfWork.CourseCommandRepository.GetByIdAsync(command.CourseId, ct);
        if (course is null)
            return CourseErrors.NotFound(command.CourseId);

        var created = CourseFaq.Create(
            course.Id,
            command.Title,
            command.Description,
            command.DisplayOrder);

        if (created.IsFailure)
            return created.Error;

        var faq = created.Data!;

        await unitOfWork.CourseFaqCommandRepository.AddAsync(faq, ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseFaqErrors.CreateFailed();

        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(command.CourseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.CreateFailed() : faq.Id.Value;
    }
}
