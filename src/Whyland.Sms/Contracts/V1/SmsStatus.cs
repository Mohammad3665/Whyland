namespace Whyland.Sms.Contracts.V1;

/// <summary>
/// Delivery states exposed by the API. <see cref="Sent"/> only means the simulator
/// accepted and recorded the message; it is never a real carrier acknowledgement (s11.3.5.3).
/// </summary>
public static class SmsStatus
{
    /// <summary>Accepted for processing but not yet handed to a provider.</summary>
    public const string Pending = "Pending";

    /// <summary>Recorded by the simulator as delivered.</summary>
    public const string Sent = "Sent";

    /// <summary>Rejected or failed before it could be recorded as delivered.</summary>
    public const string Failed = "Failed";
}
