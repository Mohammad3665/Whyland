namespace Whyland.Application.Common.Options;

/// <summary>
/// The super-admin account the database seeder creates (s6.6). Bound from the "Admin" configuration
/// section, so an operator who changes their phone number or password edits a setting — never code.
/// </summary>
public sealed class AdminAccountOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Admin";

    /// <summary>
    /// The number password sign-in resolves this account by. Password login is phone-only (s5.1.1),
    /// so without it there is no way to sign in as the admin and the account is not created.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Stored for contact purposes; never used to sign in.</summary>
    public string? Email { get; set; }

    /// <summary>Given name of the account.</summary>
    public string FirstName { get; set; } = "Whyland";

    /// <summary>Family name of the account.</summary>
    public string LastName { get; set; } = "Admin";

    /// <summary>
    /// Development default password, used only when the account is created for the first time.
    /// Rotate it through the change-password page; the seeder never overwrites an existing password.
    /// </summary>
    public string Password { get; set; } = "Whyland@1234";
}
