using Whyland.Domain.Model.Authorization.Permission.Errors;
using Whyland.Domain.Model.Authorization.Role.Errors;
using Whyland.Shared.Results;

using RolePermissionEntity = Whyland.Domain.Model.Authorization.RolePermission.Entities.RolePermission;

namespace Whyland.Domain.Model.Authorization.Permission.Entities;

/// <summary>
/// A named authorization capability identified by a unique key (s6.2).
/// Permissions are dynamic database records, not hard-coded values.
/// </summary>
public class Permission : BaseEntity<PermissionId>, ISoftDeletable
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

    #region Construction

    /// <summary>ORM constructor. New records are built through <see cref="Create"/>.</summary>
    private Permission() { }

    /// <summary>
    /// Creates a permission after trimming, validating and assigning the inputs. The identifier comes from the
    /// database sequence and the audit columns from the interceptor, so neither is set here.
    /// </summary>
    /// <param name="name">Human-readable name (e.g. "Course Creation"). Will be trimmed.</param>
    /// <param name="key">Unique authorization key (e.g. "course.create"). Will be trimmed.</param>
    /// <param name="isSystem">Whether the permission is application-defined and protected against unrestricted deletion.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new permission,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Permission> Create(string name, string key, bool isSystem = true)
    {
        var permission = new Permission();

        var applied = permission.Apply(name, key, isSystem);
        return applied.IsFailure ? applied.Error : permission;
    }

    /// <summary>
    /// Trims, validates and assigns every editable value. Uniqueness of the key belongs to the store,
    /// not here — an entity cannot see the other rows; on failure the instance is left unchanged.
    /// </summary>
    /// <param name="name">Human-readable name. Will be trimmed.</param>
    /// <param name="key">Unique authorization key. Will be trimmed.</param>
    /// <param name="isSystem">Whether the permission is application-defined.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string name, string key, bool isSystem)
    {
        var trimmedName = name?.Trim();
        var trimmedKey = key?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            return PermissionErrors.FieldRequired(nameof(Name));

        if (trimmedName.Length > MaxNameLength)
            return PermissionErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(trimmedKey))
            return PermissionErrors.FieldRequired(nameof(Key));

        if (trimmedKey.Length > MaxKeyLength)
            return PermissionErrors.FieldTooLong(nameof(Key), MaxKeyLength);

        Name = trimmedName;
        Key = trimmedKey;
        IsSystem = isSystem;

        return Result.Success();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Human-readable permission name (e.g. "Course Creation").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Unique permission key used for authorization checks (e.g. "course.create").
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// System permissions are defined by the application and protected against
    /// unrestricted deletion.
    /// </summary>
    public bool IsSystem { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public UserId? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The roles that have been granted this permission.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; } = [];

    #endregion
}
