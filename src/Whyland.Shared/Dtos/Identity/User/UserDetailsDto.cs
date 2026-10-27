namespace Whyland.Shared.Dtos.Identity.User;

/// <summary>
/// Everything the admin details modal shows about one account: identity fields, the
/// password state, the assigned roles, the profile data and the login-security counters.
/// Dates stay UTC and are rendered as Persian dates by the page script.
/// </summary>
/// <param name="Id">The user identifier.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="Email">Stored e-mail, when present.</param>
/// <param name="PhoneNumber">Stored phone number, when present.</param>
/// <param name="IsActive">Whether the account may authenticate.</param>
/// <param name="HasPassword">Whether a local password is stored.</param>
/// <param name="Roles">Display titles of the assigned roles; empty means an ordinary user.</param>
/// <param name="RoleIds">Ids of the assigned roles, so the edit modal preselects them.</param>
/// <param name="Avatar">Avatar path/URL from the profile, when set.</param>
/// <param name="BirthDate">Date of birth as "yyyy-MM-dd" (Gregorian), when set.</param>
/// <param name="Gender">Profile gender as the enum name ("Male"/"Female"), when set.</param>
/// <param name="Providers">External login providers linked to the account (e.g. "Google").</param>
/// <param name="LastLoginAt">UTC time of the last successful login, when any.</param>
/// <param name="FailedLoginAttempts">Consecutive failed password logins (s15.1.4).</param>
/// <param name="LockoutUntil">UTC time until password login is blocked, when locked.</param>
/// <param name="CreatedAt">UTC time the account was created.</param>
/// <param name="UpdatedAt">UTC time of the last modification, when any.</param>
public sealed record UserDetailsDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    bool HasPassword,
    IReadOnlyList<string> Roles,
    IReadOnlyList<long> RoleIds,
    string? Avatar,
    string? BirthDate,
    string? Gender,
    IReadOnlyList<string> Providers,
    DateTime? LastLoginAt,
    int FailedLoginAttempts,
    DateTime? LockoutUntil,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
