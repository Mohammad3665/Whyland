using Whyland.Domain.Entities.Identity;

namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// A single-use token issued by the "forgot password" flow, storing only the token's hash (s5.4).
/// </summary>
public class PasswordResetToken : BaseEntity<Guid>
{
    #region Properties

    /// <summary>
    /// The user whose password is being reset.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hash of the issued token; the plaintext token is never persisted (s5.4).
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// UTC time at which the token becomes invalid.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Set once the token has been validated and the password changed (s5.4.5).
    /// </summary>
    public bool IsUsed { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The user whose password is being reset.
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
