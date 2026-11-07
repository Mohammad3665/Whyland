using Whyland.Shared.Dtos.Catalog.Course;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetFormOptions;

/// <summary>Handles <see cref="GetCourseFormOptionsQuery"/> through the query repository.</summary>
public sealed class GetCourseFormOptionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCourseFormOptionsQueryHandler> logger) :
    IQueryHandler<GetCourseFormOptionsQuery, CourseFormOptionsDto>
{
    /// <summary>Returns both option lists, or an unexpected error when the read fails.</summary>
    public async Task<Result<CourseFormOptionsDto>> Handle(
        GetCourseFormOptionsQuery query,
        CancellationToken ct)
    {
        try
        {
            var options = await unitOfWork.CourseQueryRepository.GetFormOptionsAsync(ct);
            return Result<CourseFormOptionsDto>.Success(options);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading course form options failed.");
            return Result<CourseFormOptionsDto>.Failure(CourseErrors.OptionsFailed());
        }
    }
}
