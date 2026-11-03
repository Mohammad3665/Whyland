
using PermissionEntity = Whyland.Domain.Model.Authorization.Permission.Entities.Permission;
using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Domain.Model.Authorization.RolePermission.Entities;

/// <summary>
/// Many-to-many relation between roles and permissions (s6.3).
/// A role can have many permissions; a permission can be assigned to many roles.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class RolePermission
{
    #region Construction

    /// <summary>ORM constructor. New records are built through <see cref="Create"/>.</summary>
    private RolePermission() { }

    /// <summary>
    /// Grants a permission to a role. This row is not <see cref="IAuditable"/>, so its own
    /// timestamp is taken from the property initialiser rather than from the audit interceptor.
    /// </summary>
    /// <param name="roleId">The role receiving the permission.</param>
    /// <param name="permissionId">The permission granted to the role.</param>
    public static RolePermission Create(RoleId roleId, PermissionId permissionId) =>
        new()
        {
            RoleId = roleId,
            PermissionId = permissionId,
        };

    #endregion

    #region Properties

    /// <summary>
    /// The role receiving the permission.
    /// </summary>
    public RoleId RoleId { get; private set; }

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public PermissionId PermissionId { get; private set; }

    /// <summary>
    /// UTC time the permission was granted to the role.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    #endregion

    #region Relations

    /// <summary>
    /// The role receiving the permission.
    /// </summary>
    public RoleEntity Role { get; private set; } = null!;

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public PermissionEntity Permission { get; private set; } = null!;

    #endregion
}
