namespace Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;

/// <summary>
/// Handles <see cref="GetCourseFaqsQuery"/> by loading the questions of one course through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to course-FAQ query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetCourseFaqsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseFaqsQueryHandler> logger)
    : IQueryHandler<GetCourseFaqsQuery, IReadOnlyList<CourseFaqListItemDto>>
{
    /// <summary>
    /// Returns the questions ordered by DisplayOrder, or an error when the lookup fails.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query selecting the course whose questions are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The read-only list of questions, or an error.</returns>
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
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading FAQs of course {CourseId} failed.", query.CourseId);
            return Result<IReadOnlyList<CourseFaqListItemDto>>.Failure(CourseFaqErrors.ListFailed());
        }
    }
}
