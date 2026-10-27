namespace Whyland.Web.ViewModels;

/// <summary>
/// The "forgot password" flow on one page (s5.4): the number, then the six-digit code texted to it,
/// then the new password. The answer to the first step never reveals whether an account exists.
/// </summary>
public sealed class ForgotPasswordViewModel
{
    /// <summary>The account's phone number — recovery is resolved by number only.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>The six-digit recovery code.</summary>
    public string? Code { get; set; }

    /// <summary>The replacement password.</summary>
    public string? NewPassword { get; set; }

    /// <summary>Client-side confirmation of the new password.</summary>
    public string? ConfirmPassword { get; set; }
}

/// <summary>
/// The signed-in account stores its first password. The phone number and its SMS code are asked for
/// only when the account has none yet — which is the case for every Google-provisioned account,
/// because the number exists so that a forgotten password can be recovered (s5.4).
/// </summary>
public sealed class SetPasswordViewModel
{
    /// <summary>The number to bind; ignored by the server when the account already has one.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>The SMS code proving <see cref="PhoneNumber"/> belongs to this user.</summary>
    public string? Code { get; set; }

    /// <summary>The password to store.</summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Client-side confirmation of the new password.</summary>
    public string? ConfirmPassword { get; set; }

    /// <summary>Rendered from the account: true when the phone fields have to be shown.</summary>
    public bool RequiresPhoneNumber { get; set; }
}

/// <summary>
/// The signed-in password change (s13.2.3). Only an account that already has a password reaches it:
/// the current password is mandatory and no phone number is collected here. The user id comes from
/// the auth cookie, never from the form, so one session can never target another account.
/// </summary>
public sealed class ChangePasswordViewModel
{
    /// <summary>The stored password, proved before the new one is written.</summary>
    public string? CurrentPassword { get; set; }

    /// <summary>The replacement password.</summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>Client-side confirmation of the new password.</summary>
    public string? ConfirmPassword { get; set; }
}

/// <summary>
/// Requesting an SMS code for the phone number that is being attached to the signed-in account.
/// </summary>
public sealed class SendPhoneCodeViewModel
{
    /// <summary>The number to bind, in any Persian/international spelling.</summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
