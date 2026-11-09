namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;

/// <summary>
/// Handles <see cref="DeleteCourseFaqCommand"/> by soft-deleting the question through the command
/// repository and compacting the order of its course via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course-FAQ persistence.</param>
public sealed class DeleteCourseFaqCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteCourseFaqCommand>
{
    /// <summary>
    /// Soft-deletes the requested question, or returns an error when it does not exist or
    /// persistence fails.
    /// </summary>
    /// <param name="command">The command containing the question identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(DeleteCourseFaqCommand command, CancellationToken ct)
    {
        var faq = await unitOfWork.CourseFaqCommandRepository.GetByIdAsync(command.Id, ct);
        if (faq is null)
            return CourseFaqErrors.NotFound(command.Id);

        var courseId = faq.CourseId;

        unitOfWork.CourseFaqCommandRepository.SoftDelete(faq);

        // First save persists the soft delete so the compaction below does not
        // include (and renumber around) the deleted row.
        var saveResult = await unitOfWork.SaveAsync(ct);
        if (saveResult.IsFailure)
            return CourseFaqErrors.SoftDeleteFailed();

        // Close any DisplayOrder gap left behind, then persist the new numbering.
        await unitOfWork.CourseFaqCommandRepository.RenumberAsync(courseId, ct);

        saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.SoftDeleteFailed() : Result.Success();
    }
}
