namespace Whyland.Application.Features.Admin.Authorization.Roles.Commands.SavePermissions;

/// <summary>
/// Makes a custom role's permission set exactly the identifiers submitted by the roles editor.
/// <para>
/// The whole set is sent every time rather than a delta, because roles are the only authorization
/// mechanism: an incremental add/remove would let a stale checkbox in a half-opened modal decide
/// what an account may do. An empty list is a legal answer — it revokes everything.
/// </para>
/// </summary>
/// <param name="RoleId">The role whose grants are rewritten.</param>
/// <param name="PermissionIds">Every permission the role should end up holding.</param>
public sealed record SaveRolePermissionsCommand(
    RoleId RoleId,
    List<PermissionId> PermissionIds) : ICommand;
