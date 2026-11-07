using Whyland.Shared.Dtos.Catalog.Course;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetById;

/// <summary>
/// Handles <see cref="GetCourseByIdQuery"/> by reading the course from the query repository and
/// turning a missing row into a not-found error.
/// </summary>
public sealed class GetCourseByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseByIdQueryHandler> logger) : IQueryHandler<GetCourseByIdQuery, CourseDetailsDto>
{
    /// <summary>Returns the course, or a not-found/unexpected error.</summary>
    public async Task<Result<CourseDetailsDto>> Handle(GetCourseByIdQuery query, CancellationToken ct)
    {
        try
        {
            var course = await unitOfWork.CourseQueryRepository.GetByIdAsync(query.Id, ct);

            return course is null
                ? Result<CourseDetailsDto>.Failure(CourseErrors.NotFound(query.Id))
                : Result<CourseDetailsDto>.Success(course);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading course {CourseId} failed.", query.Id);
            return Result<CourseDetailsDto>.Failure(CourseErrors.DetailsFailed());
        }
    }
}
