
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

    public string? NationalCode { get; set; }

    public DateOnly? BirthDate { get; set; }

    public Gender? Gender { get; set; }

    #endregion

    #region Contact

    public string? Avatar { get; set; }

    public string? Address { get; set; }

    public string? Province { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    #endregion
}
