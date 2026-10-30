namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// A dynamic, manageable collection of permissions (s6.1).
/// Roles must never be hard-coded as the authorization mechanism;
/// authorization is evaluated through permission keys (s6.5).
/// </summary>
public class Role : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Name"/>; also the database column size.</summary>
    public const int MaxNameLength = 100;

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 100;

    #endregion
    /// <summary>
    /// Unique role name (e.g. "SuperAdmin").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display title (e.g. "System Administrator").
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// System roles are required by the application and protected against unrestricted
    /// deletion/modification (s6.1.3).
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// The role assignments made to users through this role.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = [];

    /// <summary>
    /// The permissions granted to this role.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
