namespace Whyland.Application.Features.Auth.Password.ForgotPassword;

/// <summary>
/// Starts the "forgot password" flow (s5.4): a six-digit code of the <c>PasswordReset</c> purpose is
/// texted to the account's number. Recovery is resolved by phone number only — an email address is
/// never a login or recovery identifier.
/// The answer given to the caller never reveals whether the number belongs to an account.
/// </summary>
/// <param name="PhoneNumber">The number entered by the user; normalized before the lookup.</param>
public sealed record ForgotPasswordCommand(string PhoneNumber) : ICommand;
