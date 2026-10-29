namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Completes the "forgot password" flow (s5.4): proves the six-digit code sent to the number and
/// writes the new password in the same request, so no intermediate token or link exists anywhere.
/// </summary>
/// <param name="PhoneNumber">The number the code was sent to.</param>
/// <param name="Code">The six-digit code, already reduced to Latin digits by the web layer; never logged or persisted in clear.</param>
/// <param name="NewPassword">The plaintext password; never logged or persisted.</param>
public sealed record ForgotPasswordConfirmCommand(string PhoneNumber, string Code, string NewPassword) : ICommand;
