namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>Renumbers the FAQs of one course in the requested visual order.</summary>
public sealed class ReorderCourseFaqsCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ReorderCourseFaqsCommand>
{
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
