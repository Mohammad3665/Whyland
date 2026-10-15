using Whyland.Domain.Entities.Identity;

namespace Whyland.Domain.Entities.Authorization;

/// <summary>
/// Many-to-many relation between users and roles (s6.4).
/// A user can have multiple roles; a role can be assigned to many users.
/// Duplicate assignment is prevented by a unique composite constraint.
/// </summary>
public class UserRole
{
    public Guid UserId { get; set; }

    public long RoleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
