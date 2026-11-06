using Whyland.Shared.Dtos.Authorization.Role;

namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetRolesForDataTableQuery"/> by delegating to the Dapper-backed role query
/// repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to role query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetRolesForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetRolesForDataTableQueryHandler> logger) :
    IQueryHandler<GetRolesForDataTableQuery, DataTableResult<RoleListItemDto>>
{
    /// <summary>
    /// Runs the listing and returns the paged result; a database failure is logged and masked behind
    /// <see cref="RoleErrors.ListFailed"/> instead of bubbling up.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged rows, or an error.</returns>
    public async Task<Result<DataTableResult<RoleListItemDto>>> Handle(
        GetRolesForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.RoleQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);
            return Result<DataTableResult<RoleListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for roles (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<RoleListItemDto>>.Failure(RoleErrors.ListFailed());
        }
    }
}
