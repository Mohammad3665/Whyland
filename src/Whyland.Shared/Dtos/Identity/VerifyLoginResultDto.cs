namespace Whyland.Shared.Dtos.Identity;

/// <summary>
/// The outcome of verifying a login code (s5.1.4, s5.3).
/// <para>
/// A verified code has exactly two possible results:
/// <list type="bullet">
///   <item><description>An <b>existing</b> account → the user is signed in; <see cref="User"/> is populated.</description></item>
///   <item><description>A <b>new</b> phone number → a short-lived <see cref="RegistrationTicket"/> is issued so the
///   client can collect the profile (name/password) and complete registration in a second step.</description></item>
/// </list>
/// </para>
/// </summary>
/// <param name="User">The authenticated user when the phone already belongs to an account; otherwise null.</param>
/// <param name="RegistrationTicket">A single-use ticket proving the phone was verified, for new accounts only.</param>
public sealed record VerifyLoginResultDto(
    AuthenticatedUserDto? User,
    string? RegistrationTicket
)
{
    /// <summary>True when the verified phone has no account yet and registration must be completed.</summary>
    public bool RequiresRegistration => User is null;
}
