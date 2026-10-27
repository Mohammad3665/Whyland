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
    public int OtpExpiryMinutes { get; set; } = 2;

    /// <summary>Maximum verification attempts before an OTP is exhausted.</summary>
    public int OtpMaxAttempts { get; set; } = 5;

    /// <summary>Minimum seconds between two OTP requests for the same number (rate limit).</summary>
    public int OtpResendCooldownSeconds { get; set; } = 60;

    /// <summary>Template code used by the SMS provider to render the OTP message.</summary>
    public string OtpSmsTemplateCode { get; set; } = "SMS-OTP-001";

    /// <summary>Minimum length accepted for any locally chosen password (s5.1.1).</summary>
    public int PasswordMinLength { get; set; } = 8;

    /// <summary>Failed password logins tolerated before the account is temporarily locked (s15.1.4).</summary>
    public int PasswordMaxFailedAttempts { get; set; } = 5;

    /// <summary>How long an account stays locked after the failed-attempt threshold is reached.</summary>
    public int PasswordLockoutMinutes { get; set; } = 15;

    /// <summary>Google OAuth client id; when empty the external login is not registered at all (s5.4).</summary>
    public string? GoogleClientId { get; set; }

    /// <summary>Google OAuth client secret. Must come from user-secrets or environment, never from a committed file.</summary>
    public string? GoogleClientSecret { get; set; }
}