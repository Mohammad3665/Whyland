namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// Why a one-time code was sent. A code of one purpose must never be accepted for another, so the
/// purpose is stored on the row and every lookup filters on it (s5.1.4, s5.4).
/// </summary>
public enum OtpPurpose
{
    /// <summary>Signing in with a phone number — also the purpose of codes issued before this existed.</summary>
    Login = 0,

    /// <summary>Proving ownership of a phone number that is being attached to the signed-in account.</summary>
    PhoneBinding = 1,

    /// <summary>Recovering the password of an account that is already reached by this number.</summary>
    PasswordReset = 2,
}
