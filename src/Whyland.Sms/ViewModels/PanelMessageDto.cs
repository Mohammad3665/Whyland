using Whyland.Sms.Storage;

namespace Whyland.Sms.ViewModels;

/// <summary>
/// A message row exactly as the panel renders it. Display strings (Persian date, time, preview,
/// badge classes) are computed on the server so a live row is byte-identical to a server-rendered
/// one and the browser needs no calendar or truncation logic. The full message body is not sent —
/// the row already links to the detail page for that.
/// </summary>
public sealed record PanelMessageDto(
    string MessageId,
    string Recipient,
    string Preview,
    string ShortPreview,
    string? TemplateCode,
    string Status,
    string StatusLabel,
    string StatusTone,
    string CreatedAt,
    string CreatedTime,
    string DetailsUrl)
{
    /// <summary>Projects a stored message onto the live row payload.</summary>
    public static PanelMessageDto From(SmsMessageRecord message)
    {
        var badge = PanelStatus.BadgeOf(message.Status);

        return new PanelMessageDto(
            message.MessageId,
            message.Recipient,
            PanelFormat.Preview(message.Message),
            PanelFormat.Preview(message.Message, 60),
            message.TemplateCode,
            message.Status,
            badge.Label,
            badge.Tone,
            PanelFormat.Utc(message.CreatedAt),
            PanelFormat.UtcTime(message.CreatedAt),
            $"/Messages/Details/{Uri.EscapeDataString(message.MessageId)}");
    }
}