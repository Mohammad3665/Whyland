using Microsoft.AspNetCore.SignalR;

namespace Whyland.Sms.Hubs;

/// <summary>
/// One-way feed for the dashboard: the server pushes, the browser never sends. Clients that are
/// not holding the panel cookie are refused at handshake — the API key is a machine credential and
/// must never open a socket that streams stored OTP text (s11.3).
/// </summary>
[Authorize(AuthenticationSchemes = PanelAuthentication.Scheme, Policy = PanelAuthentication.Policy)]
public sealed class SmsPanelHub : Hub
{
    /// <summary>Name of the event carrying one newly stored message.</summary>
    public const string MessageStoredEvent = "MessageStored";
}