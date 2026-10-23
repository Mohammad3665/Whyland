using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Whyland.Application.Common.Interfaces.Services;
using Whyland.Application.Common.Options;

namespace Whyland.Infrastructure.Services;

/// <summary>
/// Issues and consumes single-use registration tickets using an in-memory store (s5.3).
/// <para>
/// Each ticket is a cryptographically-random token bound to a verified phone number and an
/// absolute expiry. Consumption is atomic: the same ticket never works twice. The store is
/// process-local, which is acceptable because a ticket only bridges two consecutive steps of
/// one registration; a multi-instance deployment would back this with a shared cache instead.
/// </para>
/// </summary>
/// <param name="options">Auth options (ticket lifetime mirrors the login-code expiry).</param>
public sealed class RegistrationTicketService(IOptions<AuthOptions> options) : IRegistrationTicketService
{
    private readonly ConcurrentDictionary<string, Entry> _tickets = new(StringComparer.Ordinal);

    private sealed record Entry(string PhoneNumber, DateTime ExpiresAt);

    /// <summary>Issues a new single-use ticket bound to the given verified phone number.</summary>
    public string Issue(string phoneNumber)
    {
        // Crypto-random, URL-safe opaque token; never derived from the phone number.
        var ticket = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        var expiry = DateTime.UtcNow.AddMinutes(options.Value.OtpExpiryMinutes);
        _tickets[ticket] = new Entry(phoneNumber, expiry);

        PurgeExpired();
        return ticket;
    }

    /// <summary>
    /// Consumes a ticket exactly once (TryRemove is atomic) and returns its phone number,
    /// or null when unknown, already used, or expired.
    /// </summary>
    public string? Consume(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
            return null;

        // TryRemove guarantees the ticket is single-use even under concurrent requests.
        if (!_tickets.TryRemove(ticket, out var entry))
            return null;

        return entry.ExpiresAt > DateTime.UtcNow ? entry.PhoneNumber : null;
    }

    /// <summary>Best-effort cleanup so the dictionary does not grow across many abandoned flows.</summary>
    private void PurgeExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var pair in _tickets)
            if (pair.Value.ExpiresAt <= now)
                _tickets.TryRemove(pair.Key, out _);
    }
}
