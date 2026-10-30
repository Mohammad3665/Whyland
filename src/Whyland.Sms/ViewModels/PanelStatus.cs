namespace Whyland.Sms.ViewModels;

/// <summary>
/// The single place a status becomes a colored badge. Both the Razor partial and the live
/// SignalR payload read it, so a row pushed to the browser can never disagree with a row the
/// server rendered.
/// </summary>
public static class PanelStatus
{
    /// <param name="Label">Persian text shown inside the badge.</param>
    /// <param name="Tone">Tailwind background/foreground classes.</param>
    public sealed record Badge(string Label, string Tone);

    private static readonly Dictionary<string, Badge> Badges = new(StringComparer.OrdinalIgnoreCase)
    {
        [SmsStatus.Sent] = new("ارسال شد", "bg-success-light text-success"),
        [SmsStatus.Pending] = new("در انتظار", "bg-warning-light text-warning"),
        [SmsStatus.Failed] = new("ناموفق", "bg-danger-light text-danger"),
    };

    /// <summary>Badge for any stored status string, falling back to the raw value.</summary>
    public static Badge BadgeOf(string? status) =>
        status is not null && Badges.TryGetValue(status, out var badge)
            ? badge
            : new(string.IsNullOrWhiteSpace(status) ? "—" : status, "bg-light text-body");
}