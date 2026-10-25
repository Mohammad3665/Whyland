namespace Whyland.Sms.Provider;

/// <summary>
/// Connection settings for the SMS service, bound from configuration (s11.3.8.1).
/// The base address and API key are never hard-coded.
/// </summary>
public sealed class SmsProviderOptions
{
    /// <summary>Default configuration section name.</summary>
    public const string SectionName = "SmsProvider";

    /// <summary>Header the SMS service expects the shared key in.</summary>
    public const string ApiKeyHeaderName = "X-Sms-Api-Key";

    /// <summary>Root URL of the SMS service, e.g. http://localhost:5090.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Shared API key sent on every request.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Request timeout; network failure beyond it is reported as indeterminate.</summary>
    public int TimeoutSeconds { get; set; } = 15;
}
