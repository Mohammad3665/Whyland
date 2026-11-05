namespace Whyland.Shared.Dtos.Identity.User;

/// <summary>
/// The signed-in account exactly as a header user menu needs it. Deliberately narrower than the
/// admin details read: the menu renders on every page, so its row stays a single light query.
/// </summary>
/// <param name="FirstName">Given name as stored.</param>
/// <param name="LastName">Family name as stored.</param>
/// <param name="PhoneNumber">The number the account signs in with; null for an account that has none.</param>
/// <param name="Avatar">Avatar path/URL from the profile; null means the menu shows the fallback icon.</param>
public sealed record UserMenuDto(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Avatar);
