namespace Whyland.Web.ViewModels;

/// <summary>
/// A single view model for the whole one-page login/registration flow (s5.1.4, s5.3).
/// The template presents three steps in one page; <see cref="Step"/> (1/2/3) tells the view
/// which section to render. Validation is enforced per-step by the Application-layer
/// validators of each command, so this model carries no cross-step DataAnnotations.
/// </summary>
public sealed class LoginViewModel
{
    /// <summary>The active step: 1 = phone entry, 2 = code entry, 3 = profile completion.</summary>
    public int Step { get; set; } = 1;

    /// <summary>The phone number the code is sent to (carried across steps 1→2).</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>The six-digit code (step 2).</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Single-use ticket proving the phone was verified (step 3 only).</summary>
    public string Ticket { get; set; } = string.Empty;

    /// <summary>First name for a brand-new account (step 3).</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name for a brand-new account (step 3).</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Optional password for a brand-new account (step 3).</summary>
    public string? Password { get; set; }

    /// <summary>Client-side password confirmation (step 3).</summary>
    public string? ConfirmPassword { get; set; }
}
