namespace Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;

/// <summary>Reads the sections of one course and masks infrastructure failures.</summary>
public sealed class GetCourseSectionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseSectionsQueryHandler> logger)
    : IQueryHandler<GetCourseSectionsQuery, IReadOnlyList<CourseSectionListItemDto>>
{
    public async Task<Result<IReadOnlyList<CourseSectionListItemDto>>> Handle(
        GetCourseSectionsQuery query,
        CancellationToken ct)
    {
        try
        {
            var sections = await unitOfWork.CourseSectionQueryRepository.GetForCourseAsync(query.CourseId, ct);
            return Result<IReadOnlyList<CourseSectionListItemDto>>.Success(sections);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading sections of course {CourseId} failed.", query.CourseId);
            return Result<IReadOnlyList<CourseSectionListItemDto>>.Failure(CourseSectionErrors.ListFailed());
        }
    }
}
