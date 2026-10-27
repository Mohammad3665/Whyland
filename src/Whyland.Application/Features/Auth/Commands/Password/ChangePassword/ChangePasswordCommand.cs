namespace Whyland.Application.Features.Auth.Password.ChangePassword;

/// <summary>
/// Replaces the password of an account that already has one (s13.2.3). Setting a first password is a
/// different responsibility and lives in <c>SetPasswordCommand</c>.
/// The user id comes from the auth cookie, never from the request body, so one session can never
/// target another account.
/// </summary>
/// <param name="UserId">The signed-in user, taken from the cookie claims by the web layer.</param>
/// <param name="CurrentPassword">The stored password, proved before the new one is written.</param>
/// <param name="NewPassword">The replacement password; never logged or persisted in plaintext.</param>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : ICommand;
