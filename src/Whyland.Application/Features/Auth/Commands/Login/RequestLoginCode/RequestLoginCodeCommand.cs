namespace Whyland.Application.Features.Auth.Login.RequestLoginCode;

/// <summary>
/// Requests a one-time login code be sent to the given phone number (s5.1.4).
/// Succeeds silently even when the phone belongs to no account (anti-enumeration);
/// the account decision is deferred to <c>VerifyLoginCode</c>.
/// </summary>
/// <param name="PhoneNumber">The raw phone number the code is sent to.</param>
public sealed record RequestLoginCodeCommand(string PhoneNumber) : ICommand;
