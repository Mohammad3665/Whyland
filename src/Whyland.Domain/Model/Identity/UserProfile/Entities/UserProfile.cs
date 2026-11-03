using Whyland.Domain.Model.Identity.UserProfile.Enums;

using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;

namespace Whyland.Domain.Model.Identity.UserProfile.Entities;

/// <summary>
/// Extended profile data, deliberately separated from authentication data (s5.1.2).
/// Only information strictly necessary for current business requirements is collected.
/// </summary>
public class UserProfile : BaseEntity<UserProfileId>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Avatar"/>; also the database column size.</summary>
    public const int MaxAvatarLength = 256;

    #endregion

    #region Identity

    /// <summary>
    /// One-to-one with the authentication user; shares the user's identifier.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The owning user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    #endregion

    #region Personal

    /// <summary>
    /// Optional date of birth collected in the profile.
    /// </summary>
    public DateOnly? BirthDate { get; private set; }

    /// <summary>
    /// Optional gender information (s5.1.2).
    /// </summary>
    public Gender? Gender { get; private set; }

    #endregion

    #region Contact

    /// <summary>
    /// Path or URL of the profile avatar image, if provided.
    /// </summary>
    public string? Avatar { get; private set; }

    #endregion
}
