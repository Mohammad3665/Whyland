namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// The link between a user account and an external identity provider (s5.3).
/// Kept separate from <see cref="BaseEntity"/>-style entities because it has no
/// soft-delete requirement; deleting the link simply removes the login option.
/// </summary>
public class ExternalLogin
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="ProviderKey"/>; also the database column size.</summary>
    public const int MaxProviderKeyLength = 256;

    /// <summary>Maximum allowed length of <see cref="Provider"/>; also the database column size.</summary>
    public const int MaxProviderLength = 100;

    #endregion

    #region Properties

    /// <summary>
    /// The unique identifier of the login link.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The locally registered user this provider account maps to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Name of the authentication scheme (e.g. "Google").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The external provider's unique user identifier.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// UTC time the external login was first linked.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The locally registered user this provider account maps to.
    /// </summary>
    public User User { get; set; } = null!;

    #endregion
}
