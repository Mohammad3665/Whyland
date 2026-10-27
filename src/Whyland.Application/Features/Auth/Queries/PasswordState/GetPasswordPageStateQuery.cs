namespace Whyland.Application.Features.Auth.Queries.PasswordState;

/// <summary>
/// Which of the two signed-in password pages an account belongs to, decided by the account itself
/// rather than by the link the user clicked (s5.4, s13.2.3).
/// </summary>
/// <param name="RequiresPhoneNumber">True when the account has no phone number yet, so one must be
/// entered and proved by SMS before a first password can be stored.</param>
/// <param name="HasPassword">True when a password already exists, so the change page applies and the
/// current one has to be proven; false for an account that is still setting its first password.</param>
public sealed record PasswordPageStateDto(
    bool RequiresPhoneNumber,
    bool HasPassword);

/// <summary>Reads the account state that decides between the set-password and change-password pages.</summary>
/// <param name="UserId">The signed-in user, taken from the cookie claims by the web layer.</param>
public sealed record GetPasswordPageStateQuery(Guid UserId) : IQuery<PasswordPageStateDto>;
