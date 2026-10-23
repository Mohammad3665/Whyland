using Whyland.Shared.Dtos.Identity;

namespace Whyland.Application.Features.Auth.Registration.CompleteRegistration;

/// <summary>
/// Completes registration for a phone that was verified by a login code but has no account yet (s5.3).
/// The <paramref name="Ticket"/> proves the phone was verified; it is single-use and short-lived.
/// The account is created and the user is signed in in one step.
/// </summary>
/// <param name="Ticket">The single-use registration ticket returned by the verify step.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
/// <param name="Password">An optional password; when empty the account stays OTP-only.</param>
public sealed record CompleteRegistrationCommand(string Ticket, string FirstName, string LastName, string? Password)
    : ICommand<AuthenticatedUserDto>;
