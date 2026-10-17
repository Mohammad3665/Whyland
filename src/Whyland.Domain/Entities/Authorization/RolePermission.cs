namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// Many-to-many relation between roles and permissions (s6.3).
/// A role can have many permissions; a permission can be assigned to many roles.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class RolePermission
{
    /// <summary>
    /// The role receiving the permission.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public long PermissionId { get; set; }

    /// <summary>
    /// UTC time the permission was granted to the role.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The role receiving the permission.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// The permission granted to the role.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
