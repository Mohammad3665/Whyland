using Whyland.Shared.Dtos.Catalog.Course;
using Whyland.Shared.Filters;

namespace Whyland.Application.Features.Admin.Courses.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetCoursesForDataTableQuery"/> by delegating the page lookup to the
/// Dapper-backed query repository.
/// </summary>
public sealed class GetCoursesForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetCoursesForDataTableQueryHandler> logger) :
    IQueryHandler<GetCoursesForDataTableQuery, DataTableResult<CourseListItemDto>>
{
    /// <summary>
    /// Runs the DataTable listing and masks infrastructure failures behind a generic error instead
    /// of bubbling them up as exceptions.
    /// </summary>
    public async Task<Result<DataTableResult<CourseListItemDto>>> Handle(
        GetCoursesForDataTableQuery query,
        CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.CourseQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);

            return Result<DataTableResult<CourseListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for Courses (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<CourseListItemDto>>.Failure(CourseErrors.ListFailed());
        }
    }
}
