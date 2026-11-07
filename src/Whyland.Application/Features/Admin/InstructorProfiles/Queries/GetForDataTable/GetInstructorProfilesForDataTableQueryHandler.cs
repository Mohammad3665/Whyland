namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetForDataTable;

/// <summary>Delegates the instructor table page to the Dapper query repository.</summary>
public sealed class GetInstructorProfilesForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetInstructorProfilesForDataTableQueryHandler> logger)
    : IQueryHandler<GetInstructorProfilesForDataTableQuery, DataTableResult<InstructorProfileListItemDto>>
{
    public async Task<Result<DataTableResult<InstructorProfileListItemDto>>> Handle(
        GetInstructorProfilesForDataTableQuery query,
        CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.InstructorProfileQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);

            return Result<DataTableResult<InstructorProfileListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for InstructorProfiles (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<InstructorProfileListItemDto>>.Failure(
                InstructorProfileErrors.ListFailed());
        }
    }
}
