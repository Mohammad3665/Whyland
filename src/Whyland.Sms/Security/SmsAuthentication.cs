namespace Whyland.Sms.Security;

/// <summary>
/// Names used by the simulator's HTTP protection: an API-key scheme, the policy that
/// requires it, and the header callers must send (s11.3.11).
/// </summary>
public static class SmsAuthentication
{
    /// <summary>Authentication scheme holding the shared API key check.</summary>
    public const string Scheme = "SmsApiKey";

    /// <summary>Authorization policy applied to every versioned API endpoint.</summary>
    public const string Policy = "SmsApi";

    /// <summary>Request header carrying the shared API key.</summary>
    public const string HeaderName = "X-Sms-Api-Key";
}
