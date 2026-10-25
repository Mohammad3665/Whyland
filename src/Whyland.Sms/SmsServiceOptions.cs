namespace Whyland.Sms;

/// <summary>
/// Strongly-typed settings for the SMS simulator, bound from the "SmsService" section.
/// The API key is never stored in tracked files; it comes from configuration,
/// user-secrets or the environment (s11.3.11).
/// </summary>
public sealed class SmsServiceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "SmsService";

    /// <summary>
    /// Shared secret callers must present in the X-Sms-Api-Key header.
    /// When empty every request is rejected, even in development.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>Upper bound on kept requests/messages; the oldest are evicted first.</summary>
    public int MaxStoredMessages { get; set; } = 5_000;

    /// <summary>Longest accepted final message text, mimicking a provider limit.</summary>
    public int MaxMessageLength { get; set; } = 700;

    /// <summary>Dashboard sign-in name; comes from configuration or user-secrets, never a tracked file.</summary>
    public string? PanelUsername { get; set; }

    /// <summary>
    /// Dashboard sign-in password. Empty means the dashboard cannot be entered at all, because its
    /// tables contain the OTP text that must stay out of reach of anonymous visitors.
    /// </summary>
    public string? PanelPassword { get; set; }
}
