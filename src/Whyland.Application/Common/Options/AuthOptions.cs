namespace Whyland.Application.Common.Options;

/// <summary>
/// Strongly-typed options for phone/OTP authentication (s5.1.4).
/// Bound from the "Auth" configuration section.
/// </summary>
public sealed class AuthOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Auth";

    /// <summary>Number of digits in the generated OTP code.</summary>
    public int OtpCodeLength { get; set; } = 6;

    /// <summary>How long an OTP stays valid, in minutes.</summary>
    public int OtpExpiryMinutes { get; set; } = 5;

    /// <summary>Maximum verification attempts before an OTP is exhausted.</summary>
    public int OtpMaxAttempts { get; set; } = 5;

    /// <summary>Minimum seconds between two OTP requests for the same number (rate limit).</summary>
    public int OtpResendCooldownSeconds { get; set; } = 60;
}