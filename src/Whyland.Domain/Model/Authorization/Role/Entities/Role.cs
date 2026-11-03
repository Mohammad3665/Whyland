using Whyland.Domain.Model.Authorization.Permission.Errors;
using Whyland.Domain.Model.Authorization.Role.Errors;
using Whyland.Shared.Results;

using RolePermissionEntity = Whyland.Domain.Model.Authorization.RolePermission.Entities.RolePermission;
using UserRoleEntity = Whyland.Domain.Model.Authorization.UserRole.Entities.UserRole;

namespace Whyland.Domain.Model.Authorization.Role.Entities;

/// <summary>
/// A dynamic, manageable collection of permissions (s6.1).
/// Roles must never be hard-coded as the authorization mechanism;
/// authorization is evaluated through permission keys (s6.5).
/// </summary>
public class Role : BaseEntity<RoleId>, ISoftDeletable
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

    #region Construction

    /// <summary>ORM constructor. New records are built through <see cref="Create"/>.</summary>
    private Role() { }

    /// <summary>Builds the row from inputs <see cref="Validate"/> has already accepted.</summary>
    /// <param name="name">The trimmed name.</param>
    /// <param name="title">The trimmed display title.</param>
    /// <param name="isSystem">Whether the role is required by the application.</param>
    private Role(string name, string title, bool isSystem)
    {
        Name = name;
        Title = title;
        IsSystem = isSystem;
    }

    /// <summary>
    /// Creates a role after trimming and validating the inputs. The identifier comes from the
    /// database sequence and the audit columns from the interceptor, so neither is set here.
    /// </summary>
    /// <param name="name">Unique role name (e.g. "SuperAdmin"). Will be trimmed.</param>
    /// <param name="title">Human-readable display title (e.g. "System Administrator"). Will be trimmed.</param>
    /// <param name="isSystem">Whether the role is required by the application and protected against unrestricted deletion (s6.1.3).</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new role,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Role> Create(string name, string title, bool isSystem)
    {
        var trimmedName = name?.Trim();
        var trimmedTitle = title?.Trim();

        var validation = Validate(trimmedName, trimmedTitle);
        if (validation.IsFailure)
            return validation.Error;

        return new Role(trimmedName!, trimmedTitle!, isSystem);
    }

    /// <summary>
    /// Validates the trimmed inputs against the invariants this row can judge on its own: the two
    /// text fields and their column sizes. Uniqueness of the name belongs to the store, not here.
    /// </summary>
    /// <param name="name">The trimmed name to validate.</param>
    /// <param name="title">The trimmed title to validate.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private static Result Validate(string? name, string? title)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RoleErrors.FieldRequired(nameof(Name));

        if (name.Length > MaxNameLength)
            return RoleErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(title))
            return RoleErrors.FieldRequired(nameof(Title));

        if (title.Length > MaxTitleLength)
            return RoleErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        return Result.Success();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Unique role name (e.g. "SuperAdmin").
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable display title (e.g. "System Administrator").
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// System roles are required by the application and protected against unrestricted
    /// deletion/modification (s6.1.3).
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
    /// The role assignments made to users through this role.
    /// </summary>
    public ICollection<UserRoleEntity> UserRoles { get; } = [];

    /// <summary>
    /// The permissions granted to this role.
    /// </summary>
    public ICollection<RolePermissionEntity> RolePermissions { get; } = [];

    #endregion
}
