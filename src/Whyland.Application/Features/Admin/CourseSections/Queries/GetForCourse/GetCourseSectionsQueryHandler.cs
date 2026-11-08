namespace Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;

/// <summary>
/// Handles <see cref="GetCourseSectionsQuery"/> by loading the sections of one course through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to section query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetCourseSectionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseSectionsQueryHandler> logger)
    : IQueryHandler<GetCourseSectionsQuery, IReadOnlyList<CourseSectionListItemDto>>
{
    /// <summary>
    /// Returns the sections ordered by DisplayOrder, or an error when the lookup fails.
    /// Infrastructure failures are logged and masked behind a generic
    /// <see cref="ErrorType.Unexpected"/> error instead of bubbling up as exceptions.
    /// </summary>
    /// <param name="query">The query selecting the course whose sections are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The read-only list of sections, or an error.</returns>
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
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading sections of course {CourseId} failed.", query.CourseId);
            return Result<IReadOnlyList<CourseSectionListItemDto>>.Failure(CourseSectionErrors.ListFailed());
        }
    }
}
