namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>Soft-deletes a course FAQ and compacts the order of its course.</summary>
public sealed class DeleteCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourseFaqCommand>
{
    public async Task<Result> Handle(DeleteCourseFaqCommand command, CancellationToken ct)
    {
        var faq = await unitOfWork.CourseFaqCommandRepository.GetByIdAsync(command.Id, ct);
        if (faq is null)
            return CourseFaqErrors.NotFound(command.Id);

        var courseId = faq.CourseId.Value;

        unitOfWork.CourseFaqCommandRepository.SoftDelete(faq);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseFaqErrors.SoftDeleteFailed();

        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(courseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.SoftDeleteFailed() : Result.Success();
    }
}
