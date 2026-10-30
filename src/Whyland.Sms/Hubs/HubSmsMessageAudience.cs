using Microsoft.AspNetCore.SignalR;
using Whyland.Sms.Application;

namespace Whyland.Sms.Hubs;

/// <summary>
/// Broadcasts stored messages to every signed-in dashboard tab.
/// </summary>
/// <param name="hub">Panel hub context.</param>
public sealed class HubSmsMessageAudience(IHubContext<SmsPanelHub> hub) : ISmsMessageAudience
{
    /// <inheritdoc />
    public void MessageStored(SmsMessageRecord message)
    {
        // Fire and forget on purpose: the latency and the success of POST /api/v1/sms/send must not
        // depend on how many dashboards happen to be open, or on a browser that stopped reading.
        _ = hub.Clients.All.SendAsync(SmsPanelHub.MessageStoredEvent, PanelMessageDto.From(message));
    }
}