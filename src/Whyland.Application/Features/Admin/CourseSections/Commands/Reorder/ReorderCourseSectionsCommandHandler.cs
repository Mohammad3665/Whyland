namespace Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;

/// <summary>Renumbers the sections of one course in the requested visual order.</summary>
public sealed class ReorderCourseSectionsCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderCourseSectionsCommand>
{
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
