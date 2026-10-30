namespace Whyland.Application.Features.Auth.Login.LoginWithPassword;

/// <summary>
/// Signs a user in with a locally stored password (s5.1.1).
/// Business policy: password sign-in resolves accounts by phone number only — an email address
/// is never a login identifier, it is only a hint used when matching an external identity.
/// </summary>
/// <param name="PhoneNumber">The verified phone number of the account.</param>
/// <param name="Password">The plaintext password; never logged or persisted.</param>
public sealed record LoginWithPasswordCommand(string PhoneNumber, string Password) : ICommand<AuthenticatedUserDto>;
