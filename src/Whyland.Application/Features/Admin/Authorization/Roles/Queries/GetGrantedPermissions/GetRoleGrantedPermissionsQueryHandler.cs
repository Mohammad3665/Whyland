namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetGrantedPermissions;

/// <summary>
/// Handles <see cref="GetRoleGrantedPermissionsQuery"/> by reading the role's grant rows through the
/// Dapper-backed query repository.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to role query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetRoleGrantedPermissionsQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetRoleGrantedPermissionsQueryHandler> logger) :
    IQueryHandler<GetRoleGrantedPermissionsQuery, IReadOnlyList<PermissionId>>
{
    /// <summary>
    /// Returns the granted identifiers, or a masked error when the read fails. A role that exists but
    /// grants nothing is a normal answer, not an error: an empty selection is what a fresh role has.
    /// </summary>
    /// <param name="query">The query carrying the role identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<Result<IReadOnlyList<PermissionId>>> Handle(
        GetRoleGrantedPermissionsQuery query, CancellationToken ct)
    {
        try
        {
            var granted = await unitOfWork.RoleQueryRepository
                .GetGrantedPermissionIdsAsync(query.RoleId, ct);
            return Result<IReadOnlyList<PermissionId>>.Success(granted);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading granted permissions failed for role {RoleId}.", query.RoleId);
            return Result<IReadOnlyList<PermissionId>>.Failure(RoleErrors.ListFailed());
        }
    }
}
