namespace Whyland.Application.Features.Auth.Password.SetPassword;

/// <summary>
/// Stores the first password of the signed-in account (s13.2.3). This is a different
/// responsibility from <c>ChangePassword</c>: there is no old password to prove, and the account is
/// expected to have none.
/// <para>
/// An account that has no phone number yet supplies one here together with the SMS code proving it,
/// because that number is the only channel a forgotten password can later be reset through.
/// </para>
/// </summary>
/// <param name="UserId">The signed-in user, taken from the cookie claims by the web layer.</param>
/// <param name="PhoneNumber">The number to bind; required only when the account has none.</param>
/// <param name="Code">The SMS code, already reduced to Latin digits by the web layer, proving
/// <paramref name="PhoneNumber"/> belongs to this user.</param>
/// <param name="NewPassword">The password to store; never logged or persisted in plaintext.</param>
public sealed record SetPasswordCommand(
    Guid UserId,
    string? PhoneNumber,
    string? Code,
    string NewPassword
) : ICommand;
