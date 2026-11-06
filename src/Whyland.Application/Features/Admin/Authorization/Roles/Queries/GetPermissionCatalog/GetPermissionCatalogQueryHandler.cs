using Whyland.Shared.Dtos.Authorization.Permission;

namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetPermissionCatalog;

/// <summary>
/// Handles <see cref="GetPermissionCatalogQuery"/> by reading the catalogue from the Dapper-backed
/// role query repository, which hides retired permission rows itself.
/// </summary>
/// <param name="unitOfWork">The unit of work providing access to role query data.</param>
/// <param name="logger">The logger used to record infrastructure failures behind the mask.</param>
public sealed class GetPermissionCatalogQueryHandler(
    IUnitOfWork unitOfWork,
    ILogger<GetPermissionCatalogQueryHandler> logger) :
    IQueryHandler<GetPermissionCatalogQuery, IReadOnlyList<PermissionOptionDto>>
{
    /// <summary>
    /// Returns the live permissions ordered by key, or a masked error when the read fails.
    /// </summary>
    /// <param name="query">The query (carries no parameters).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<Result<IReadOnlyList<PermissionOptionDto>>> Handle(
        GetPermissionCatalogQuery query, CancellationToken ct)
    {
        try
        {
            var permissions = await unitOfWork.RoleQueryRepository.GetPermissionsAsync(ct);
            return Result<IReadOnlyList<PermissionOptionDto>>.Success(permissions);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            logger.LogError(ex, "Reading the permission catalogue failed.");
            return Result<IReadOnlyList<PermissionOptionDto>>.Failure(RoleErrors.ListFailed());
        }
    }
}
