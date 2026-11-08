namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>
/// Handles <see cref="ReorderCourseFaqsCommand"/> by renumbering DisplayOrder 1..n in the given
/// visual order and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course-FAQ persistence.</param>
public sealed class ReorderCourseFaqsCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderCourseFaqsCommand>
{
    /// <summary>
    /// Persists the new display order, or returns an error when persistence fails.
    /// </summary>
    /// <param name="command">The command carrying the new ordering sequence.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(ReorderCourseFaqsCommand command, CancellationToken ct)
    {
        await unitOfWork.CourseFaqCommandRepository.ReorderAsync(
            command.CourseId,
            command.OrderedIds,
            ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseFaqErrors.ReorderFailed() : Result.Success();
    }
}
