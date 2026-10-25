using System.Globalization;

namespace Whyland.Sms.ViewModels;

/// <summary>
/// Display helpers for the dashboard. Timestamps stay in UTC and are labeled as such, because
/// the store only ever records UTC values and the panel is a debugging surface, not a report.
/// </summary>
public static class PanelFormat
{
    private static readonly PersianCalendar Persian = new();

    /// <summary>Formats a UTC instant as <c>1404/08/02 13:42:07</c> without needing ICU data.</summary>
    public static string Utc(DateTime value)
    {
        var utc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return $"{Persian.GetYear(utc)}/{Persian.GetMonth(utc):00}/{Persian.GetDayOfMonth(utc):00} "
            + $"{utc:HH:mm:ss}";
    }

    /// <summary>Time only, for dense tables.</summary>
    public static string UtcTime(DateTime value) =>
        DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    /// <summary>Two-cell text for a nullable instant.</summary>
    public static string UtcOrDash(DateTime? value) =>
        value is null ? "—" : Utc(value.Value);

    /// <summary>Trims long message bodies so a table row stays one line.</summary>
    public static string Preview(string? value, int length = 90)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "—";
        }

        var oneLine = value.ReplaceLineEndings(" ").Trim();
        return oneLine.Length <= length ? oneLine : string.Concat(oneLine.AsSpan(0, length), "…");
    }
}
