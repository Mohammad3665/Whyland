using Whyland.Shared.Dtos.Authorization.Role;

namespace Whyland.Application.Features.Admin.Users.Queries.GetRoles;

/// <summary>
/// Handles <see cref="GetAssignableRolesQuery"/> by reading the role list from the
/// user query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to user query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetAssignableRolesQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetAssignableRolesQueryHandler> logger) :
    IQueryHandler<GetAssignableRolesQuery, IReadOnlyList<RoleOptionDto>>
{
    /// <summary>
    /// Returns the assignable roles, or a masked error when the read fails.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<Result<IReadOnlyList<RoleOptionDto>>> Handle(GetAssignableRolesQuery query, CancellationToken ct)
    {
        try
        {
            var roles = await unitOfWork.UserQueryRepository.GetAssignableRolesAsync(ct);
            return Result<IReadOnlyList<RoleOptionDto>>.Success(roles);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading assignable roles failed.");
            return Result<IReadOnlyList<RoleOptionDto>>.Failure(UserErrors.RolesListFailed());
        }
    }
}
