using Whyland.Shared.Dtos.Authorization.Permission;
using Whyland.Shared.Dtos.Authorization.Role;
using Whyland.Shared.Filters;

namespace Whyland.Domain.Model.Authorization.Role.Repositories;

/// <summary>
/// Query-side repository for the Role aggregate. Implemented with Dapper over the pooled
/// <c>NpgsqlDataSource</c>: nothing here tracks entities, and every read is a statement the admin
/// screens run against the PostgreSQL tables directly.
/// </summary>
/// <remarks>
/// Dapper does not see the EF global query filter, so each statement hides soft-deleted rows itself.
/// </remarks>
public interface IRoleQueryRepository
{
    #region Roles

    /// <summary>
    /// One page of the roles catalog through the shared data-table engine: paging, the sort column
    /// and every filter chip are resolved against the whitelisted columns, so a new column is a
    /// whitelist entry rather than a new query.
    /// </summary>
    /// <param name="filter">Paging, sorting and the searched field.</param>
    /// <param name="conditions">The active filter chips, in the shape the engine understands.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<DataTableResult<RoleListItemDto>> GetForDataTableAsync(
        DataTableFilter filter,
        IReadOnlyList<FilterCondition>? conditions = null,
        CancellationToken ct = default);

    /// <summary>
    /// The permission identifiers currently granted to a role — what the editor has to pre-select.
    /// </summary>
    /// <param name="roleId">The role whose grants are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<PermissionId>> GetGrantedPermissionIdsAsync(RoleId roleId, CancellationToken ct = default);

    #endregion

    #region Permissions

    /// <summary>
    /// The live permission catalogue, ordered by key, so the role editor groups it by prefix and
    /// the groups arrive in a stable order.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    Task<IReadOnlyList<PermissionOptionDto>> GetPermissionsAsync(CancellationToken ct = default);

    #endregion
}
