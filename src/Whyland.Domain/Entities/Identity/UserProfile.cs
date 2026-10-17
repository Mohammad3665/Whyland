namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// Extended profile data, deliberately separated from authentication data (s5.1.2).
/// Only information strictly necessary for current business requirements is collected.
/// </summary>
public class UserProfile : BaseEntity<Guid>
{
    #region Identity

    /// <summary>
    /// One-to-one with the authentication user; shares the user's identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The owning user.
    /// </summary>
    public User User { get; set; } = null!;

    #endregion

    #region Personal

    /// <summary>
    /// Optional date of birth collected in the profile.
    /// </summary>
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Optional gender information (s5.1.2).
    /// </summary>
    public Gender? Gender { get; set; }

    #endregion

    #region Contact

    /// <summary>
    /// Path or URL of the profile avatar image, if provided.
    /// </summary>
    public string? Avatar { get; set; }

    #endregion
}
