namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;

/// <summary>Updates a course FAQ and compacts the order of its course.</summary>
public sealed class UpdateCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCourseFaqCommand>
{
    public async Task<Result> Handle(UpdateCourseFaqCommand command, CancellationToken ct)
    {
        var faq = await unitOfWork.CourseFaqCommandRepository.GetByIdAsync(command.Id, ct);
        if (faq is null)
            return CourseFaqErrors.NotFound(command.Id);

        var updated = faq.Update(command.Title, command.Description, command.DisplayOrder);
        if (updated.IsFailure)
            return updated.Error;

        unitOfWork.CourseFaqCommandRepository.Update(faq);

        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseFaqErrors.UpdateFailed();

        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(faq.CourseId.Value, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.UpdateFailed() : Result.Success();
    }
}
