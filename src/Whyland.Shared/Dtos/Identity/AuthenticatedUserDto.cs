namespace Whyland.Shared.Dtos.Identity;

/// <summary>
/// The identity payload produced by a successful login, consumed by the web
/// layer to build the authentication cookie's claims (s5.1.4, s6.4).
/// </summary>
/// <param name="UserId">The authenticated user's unique identifier.</param>
/// <param name="FullName">Display name for the principal.</param>
/// <param name="Permissions">Effective permission keys (become "permissions" claims).</param>
public sealed record AuthenticatedUserDto(
    Guid UserId,
    string FullName,
    IReadOnlyList<string> Permissions
);