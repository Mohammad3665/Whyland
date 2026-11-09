namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetGrantedPermissions;

/// <summary>
/// Reads the permission identifiers a role currently holds — what the permissions modal has to
/// pre-select before the operator touches the tree.
/// </summary>
/// <param name="RoleId">The role whose grants are read.</param>
public sealed record GetRoleGrantedPermissionsQuery(RoleId RoleId)
    : IQuery<IReadOnlyList<PermissionId>>;
