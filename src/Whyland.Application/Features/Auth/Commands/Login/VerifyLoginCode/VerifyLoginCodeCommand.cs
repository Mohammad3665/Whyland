namespace Whyland.Application.Features.Auth.Login.VerifyLoginCode;

/// <summary>
/// Verifies a one-time login code for a phone number (s5.1.4).
/// On success it either signs-in an existing account (returns the authenticated user)
/// or, for a brand-new phone, returns a short-lived registration ticket so the client
/// can complete registration in a follow-up step (s5.3).
/// </summary>
/// <param name="PhoneNumber">The phone number the code was issued for.</param>
/// <param name="Code">The login code entered by the user, already reduced to Latin digits by the web layer.</param>
public sealed record VerifyLoginCodeCommand(string PhoneNumber, string Code)
    : ICommand<VerifyLoginResultDto>;
