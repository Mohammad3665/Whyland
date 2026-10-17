namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// Many-to-many relation between users and roles (s6.4).
/// A user can have multiple roles; a role can be assigned to many users.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class UserRole
{
    /// <summary>
    /// The user receiving the role.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// UTC time the role was assigned to the user.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user receiving the role.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The role assigned to the user.
    /// </summary>
    public Role Role { get; set; } = null!;
}
