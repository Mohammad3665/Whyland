
using RoleEntity = Whyland.Domain.Model.Authorization.Role.Entities.Role;

namespace Whyland.Domain.Model.Authorization.UserRole.Entities;

/// <summary>
/// Many-to-many relation between users and roles (s6.4).
/// A user can have multiple roles; a role can be assigned to many users.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class UserRole
{
    #region Construction

    /// <summary>ORM constructor. New records are built through <see cref="Create"/>.</summary>
    private UserRole() { }

    /// <summary>
    /// Links a user to a role. This row is not <see cref="IAuditable"/>, so its own timestamp is
    /// taken from the property initialiser rather than from the audit interceptor.
    /// </summary>
    /// <param name="userId">The user receiving the role.</param>
    /// <param name="roleId">The role assigned to the user.</param>
    public static UserRole Create(UserId userId, RoleId roleId) =>
        new()
        {
            UserId = userId,
            RoleId = roleId,
        };

    #endregion

    #region Properties

    /// <summary>
    /// The user receiving the role.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public RoleId RoleId { get; private set; }

    /// <summary>
    /// UTC time the role was assigned to the user.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    #endregion

    #region Relations

    /// <summary>
    /// The user receiving the role.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public RoleEntity Role { get; private set; } = null!;

    #endregion
}
