namespace Whyland.Application.Common.Interfaces.Services;

/// <summary>
/// Issues and consumes single-use, short-lived tickets that prove a phone number was
/// verified by a login code but does not yet belong to any account (s5.3).
/// <para>
/// The ticket bridges the "verify code" step and the "complete registration" step so the
/// registration endpoint can trust that the phone was really verified, without re-sending
/// or re-checking the (now burned) code.
/// </para>
/// </summary>
public interface IRegistrationTicketService
{
    /// <summary>Issues a new single-use ticket bound to the given verified phone number.</summary>
    /// <param name="phoneNumber">The verified phone number the ticket represents.</param>
    /// <returns>An opaque, single-use ticket string.</returns>
    string Issue(string phoneNumber);

    /// <summary>
    /// Consumes a ticket exactly once and returns the phone number it was issued for.
    /// Returns <c>null</c> when the ticket is unknown, already used, or expired.
    /// </summary>
    /// <param name="ticket">The ticket to consume.</param>
    string? Consume(string ticket);
}
