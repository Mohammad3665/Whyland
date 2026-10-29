using Whyland.Sms.Storage;

namespace Whyland.Sms.Application;

/// <summary>
/// Notifies whoever is watching that a message has just been stored. Kept as an interface so the
/// send pipeline does not depend on SignalR.
/// </summary>
public interface ISmsMessageAudience
{
    /// <summary>Publishes one accepted message. Never throws into the send path.</summary>
    void MessageStored(SmsMessageRecord message);
}