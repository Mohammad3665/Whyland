namespace Whyland.Application.Features.Admin.Users.Commands.Create;

/// <summary>
/// Creates an account from the admin users modal and returns its identifier.
/// <para>
/// The phone number given here is <em>not</em> OTP-verified: only the public registration and
/// password-setting flows prove ownership of a number. An account created here therefore has an
/// unconfirmed recovery channel until its owner signs in and verifies it.
/// </para>
/// </summary>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="Email">Optional e-mail; unique when present.</param>
/// <param name="PhoneNumber">Optional phone number; the sign-in and recovery identifier, unique when present.</param>
/// <param name="Password">Initial plaintext password; only its hash is stored.</param>
/// <param name="IsActive">Whether the account may authenticate.</param>
/// <param name="RoleIds">Roles assigned to the account.</param>
public sealed record CreateUserCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string Password,
    bool IsActive,
    List<long> RoleIds) : ICommand<Guid>;
