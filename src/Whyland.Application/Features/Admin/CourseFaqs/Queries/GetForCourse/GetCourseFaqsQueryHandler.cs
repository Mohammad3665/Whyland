namespace Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;

/// <summary>Reads the FAQs of one course and masks infrastructure failures.</summary>
public sealed class GetCourseFaqsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseFaqsQueryHandler> logger)
    : IQueryHandler<GetCourseFaqsQuery, IReadOnlyList<CourseFaqListItemDto>>
{
    public async Task<Result<IReadOnlyList<CourseFaqListItemDto>>> Handle(
        GetCourseFaqsQuery query,
        CancellationToken ct)
    {
        try
        {
            var faqs = await unitOfWork.CourseFaqQueryRepository.GetForCourseAsync(query.CourseId, ct);
            return Result<IReadOnlyList<CourseFaqListItemDto>>.Success(faqs);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading FAQs of course {CourseId} failed.", query.CourseId);
            return Result<IReadOnlyList<CourseFaqListItemDto>>.Failure(CourseFaqErrors.ListFailed());
        }
    }
}
