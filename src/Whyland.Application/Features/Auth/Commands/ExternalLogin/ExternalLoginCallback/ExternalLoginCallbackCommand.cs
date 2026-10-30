namespace Whyland.Application.Features.Auth.ExternalLogin;

/// <summary>
/// Resolves an external provider callback (s5.4): a known provider id signs the linked account in,
/// an unknown one gets a fresh phone-less account. Either way the result is the user to authenticate.
/// </summary>
/// <param name="Identity">The identity the provider just proved, read from its claims by the web layer.</param>
public sealed record ExternalLoginCallbackCommand(ExternalIdentityDto Identity) : ICommand<AuthenticatedUserDto>;
