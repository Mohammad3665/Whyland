namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// A named authorization capability identified by a unique key (s6.2).
/// Permissions are dynamic database records, not hard-coded values.
/// </summary>
public class Permission : BaseEntity<long>
{
    /// <summary>
    /// Human-readable permission name (e.g. "Course Creation").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique permission key used for authorization checks (e.g. "course.create").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// System permissions are defined by the application and protected against
    /// unrestricted deletion.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// The roles that have been granted this permission.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
