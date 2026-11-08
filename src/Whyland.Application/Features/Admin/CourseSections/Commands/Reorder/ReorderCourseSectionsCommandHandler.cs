namespace Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;

/// <summary>
/// Handles <see cref="ReorderCourseSectionsCommand"/> by renumbering DisplayOrder 1..n in the given
/// visual order and persisting the change via the unit of work.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to section persistence.</param>
public sealed class ReorderCourseSectionsCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderCourseSectionsCommand>
{
    /// <summary>
    /// Persists the new display order, or returns an error when persistence fails.
    /// </summary>
    /// <param name="command">The command carrying the new ordering sequence.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A success result, or an error describing the failure.</returns>
    public async Task<Result> Handle(ReorderCourseSectionsCommand command, CancellationToken ct)
    {
        await unitOfWork.CourseSectionCommandRepository.ReorderAsync(
            command.CourseId,
            command.OrderedIds,
            ct);

        var saveResult = await unitOfWork.SaveAsync(ct);
        return saveResult.IsFailure ? CourseSectionErrors.ReorderFailed() : Result.Success();
    }
}
