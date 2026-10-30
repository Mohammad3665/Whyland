namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// A named authorization capability identified by a unique key (s6.2).
/// Permissions are dynamic database records, not hard-coded values.
/// </summary>
public class Permission : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Key"/>; also the database column size.</summary>
    public const int MaxKeyLength = 100;

    /// <summary>Maximum allowed length of <see cref="Name"/>; also the database column size.</summary>
    public const int MaxNameLength = 100;

    #endregion
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
