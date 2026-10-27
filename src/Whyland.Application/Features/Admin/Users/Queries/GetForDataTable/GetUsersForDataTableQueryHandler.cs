using Microsoft.Extensions.Logging;
using Whyland.Application.Features.Admin.Users.Errors;
using Whyland.Shared.Dtos.Identity.User;

namespace Whyland.Application.Features.Admin.Users.Queries.GetForDataTable;

/// <summary>
/// Handles <see cref="GetUsersForDataTableQuery"/> by delegating to the Dapper-backed
/// user query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to user query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetUsersForDataTableQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetUsersForDataTableQueryHandler> logger) :
    IQueryHandler<GetUsersForDataTableQuery, DataTableResult<UserListItemDto>>
{
    /// <summary>
    /// Runs the listing and returns the paged result; a database failure is logged and
    /// masked behind <see cref="UserAdminErrors.ListFailed"/> instead of bubbling up.
    /// </summary>
    /// <param name="query">The query carrying the DataTable filter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The paged rows, or an error.</returns>
    public async Task<Result<DataTableResult<UserListItemDto>>> Handle(GetUsersForDataTableQuery query, CancellationToken ct)
    {
        try
        {
            var result = await unitOfWork.UserQueryRepository.GetForDataTableAsync(
                query.Filter, query.Filters, ct);
            return Result<DataTableResult<UserListItemDto>>.Success(result);
        }
        catch (Exception ex)
        {
            // Cancellation is the caller's decision and must propagate.
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex,
                "Data Table listing failed for users (Draw={Draw}, Start={Start}).",
                query.Filter.Draw, query.Filter.Start);

            return Result<DataTableResult<UserListItemDto>>.Failure(UserAdminErrors.ListFailed());
        }
    }
}
