using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Whyland.Sms.Security;

/// <summary>
/// Human-facing sign-in for the dashboard. Deliberately separate from
/// <see cref="SmsAuthentication"/>: the API key authenticates services, this cookie
/// authenticates a person sitting in a browser, and neither can borrow the other's policy.
/// </summary>
public static class PanelAuthentication
{
    /// <summary>Cookie authentication scheme name.</summary>
    public const string Scheme = "SmsPanel";

    /// <summary>Authorization policy every dashboard controller uses.</summary>
    public const string Policy = "SmsPanel";

    /// <summary>Cookie name issued after a successful sign-in.</summary>
    public const string CookieName = "whyland.sms.panel";

    /// <summary>Where unauthenticated dashboard requests are sent.</summary>
    public const string SignInPath = "/Account/Login";

    /// <summary>How long a signed-in browser stays authenticated.</summary>
    public static readonly TimeSpan SessionLength = TimeSpan.FromHours(8);

    /// <summary>True when both dashboard credentials are configured.</summary>
    public static bool IsConfigured(SmsServiceOptions options) =>
        !string.IsNullOrWhiteSpace(options.PanelUsername) && !string.IsNullOrWhiteSpace(options.PanelPassword);

    /// <summary>
    /// Verifies the submitted pair against configuration. Unconfigured credentials always
    /// fail: an anonymous visitor must never read stored OTP text.
    /// </summary>
    public static bool TryVerify(SmsServiceOptions options, string? username, string? password) =>
        IsConfigured(options)
        && FixedTimeEquals(options.PanelUsername!, username)
        && FixedTimeEquals(options.PanelPassword!, password);

    /// <summary>Builds the principal stored in the auth cookie (identity only, no secrets).</summary>
    public static ClaimsPrincipal CreatePrincipal(string username)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, username)],
            Scheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static bool FixedTimeEquals(string expected, string? supplied)
    {
        if (supplied is null)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(supplied));
    }
}
