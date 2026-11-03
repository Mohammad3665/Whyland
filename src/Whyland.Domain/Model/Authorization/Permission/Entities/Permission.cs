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

    /// <summary>Builds the row from inputs <see cref="Validate"/> has already accepted.</summary>
    /// <param name="name">The trimmed name.</param>
    /// <param name="key">The trimmed authorization key.</param>
    /// <param name="isSystem">Whether the permission is application-defined.</param>
    private Permission(string name, string key, bool isSystem)
    {
        Name = name;
        Key = key;
        IsSystem = isSystem;
    }

    /// <summary>
    /// Creates a permission after trimming and validating the inputs. The identifier comes from the
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
        var trimmedName = name?.Trim();
        var trimmedKey = key?.Trim();

        var validation = Validate(trimmedName, trimmedKey);
        if (validation.IsFailure)
            return validation.Error;

        return new Permission(trimmedName!, trimmedKey!, isSystem);
    }

    /// <summary>
    /// Validates the trimmed inputs against the invariants this row can judge on its own: the two
    /// text fields and their column sizes. Uniqueness of the key belongs to the store, not here —
    /// an entity cannot see the other rows.
    /// </summary>
    /// <param name="name">The trimmed name to validate.</param>
    /// <param name="key">The trimmed key to validate.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private static Result Validate(string? name, string? key)
    {
        if (string.IsNullOrWhiteSpace(name))
            return PermissionErrors.FieldRequired(nameof(Name));

        if (name.Length > MaxNameLength)
            return PermissionErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(key))
            return PermissionErrors.FieldRequired(nameof(Key));

        if (key.Length > MaxKeyLength)
            return PermissionErrors.FieldTooLong(nameof(Key), MaxKeyLength);

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
