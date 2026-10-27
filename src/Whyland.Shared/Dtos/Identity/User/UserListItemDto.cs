namespace Whyland.Shared.Dtos.Identity.User;

/// <summary>
/// One row of the admin users table. It carries only fields that really exist on the
/// account (plus the profile picture the avatar is rendered from), so any of them can be
/// searched, filtered or sorted through the generic pipeline; everything derived (roles,
/// password state, profile, security state) belongs to <see cref="UserDetailsDto"/> and is
/// shown in the row details modal.
/// </summary>
/// <param name="Id">The user identifier.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="Email">Stored e-mail, when present.</param>
/// <param name="PhoneNumber">Stored phone number — the sign-in and recovery identifier.</param>
/// <param name="IsActive">Whether the account may authenticate.</param>
/// <param name="LastLoginAt">UTC time of the last successful login, when any.</param>
/// <param name="Avatar">Avatar path/URL from the profile; null means the row shows initials.</param>
public sealed record UserListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    DateTime? LastLoginAt,
    string? Avatar);
