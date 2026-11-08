namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;

/// <summary>
/// Handles <see cref="UpdateCourseFaqCommand"/> by loading the question, applying the domain
/// <c>Update</c> method, persisting the change via the unit of work, and compacting the order of
/// its course.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course-FAQ persistence.</param>
public sealed class UpdateCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCourseFaqCommand>
{
    /// <summary>
    /// Updates the requested question, or returns an error when it does not exist, the domain
    /// update is rejected, or persistence fails.
    /// </summary>
    /// <param name="command">The command with the updated question values.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
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

        // Compact 1..n so a client-entered order (10, 17, 100, ...) still
        // yields a clean sequence matching each question's sorted position.
        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(faq.CourseId.Value, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.UpdateFailed() : Result.Success();
    }
}
