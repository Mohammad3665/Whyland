namespace Whyland.Shared.Dtos.Identity;

/// <summary>
/// The identity an external provider (Google) proved, before any local account is involved (s5.4).
/// </summary>
/// <param name="Provider">Authentication scheme name, e.g. "Google".</param>
/// <param name="ProviderKey">The provider's stable subject id for this person.</param>
/// <param name="Email">Email claim when the provider released one; never a verification proof.</param>
/// <param name="FirstName">Given name claim, may be empty.</param>
/// <param name="LastName">Family name claim, may be empty.</param>
public sealed record ExternalIdentityDto(
    string Provider,
    string ProviderKey,
    string? Email,
    string FirstName,
    string LastName);
