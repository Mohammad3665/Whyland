namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// Many-to-many relation between roles and permissions (s6.3).
/// A role can have many permissions; a permission can be assigned to many roles.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class RolePermission
{
    public long RoleId { get; set; }

    public long PermissionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}
