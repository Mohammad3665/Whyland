namespace Whyland.Sms.Storage;

/// <summary>
/// Process-lifetime store for SMS traffic and template definitions. Deliberately
/// not persistent: restarting the simulator clears requests and messages (s11.1.4).
/// </summary>
public interface ISmsStore
{
    /// <summary>Records a produced message, evicting the oldest when the cap is reached.</summary>
    void AddMessage(SmsMessageRecord message);

    /// <summary>Records an inbound API call, evicting the oldest when the cap is reached.</summary>
    void AddRequest(SmsRequestRecord request);

    /// <summary>Looks up one message by its public id.</summary>
    SmsMessageRecord? FindMessage(string messageId);

    /// <summary>Looks up one request by its correlation id.</summary>
    SmsRequestRecord? FindRequest(Guid requestId);

    /// <summary>Newest-first message list, optionally narrowed by recipient, status or template.</summary>
    IReadOnlyList<SmsMessageRecord> ListMessages(
        string? recipient = null,
        string? status = null,
        string? templateCode = null,
        int limit = 100);

    /// <summary>Newest-first request list, optionally narrowed by recipient or status.</summary>
    IReadOnlyList<SmsRequestRecord> ListRequests(
        string? recipient = null,
        string? status = null,
        int limit = 100);

    /// <summary>Reads a template by its case-insensitive code.</summary>
    SmsTemplateRecord? FindTemplate(string code);

    /// <summary>All templates, ordered by code.</summary>
    IReadOnlyList<SmsTemplateRecord> ListTemplates();

    /// <summary>Stores a new template; false when the code is already taken.</summary>
    bool TryAddTemplate(SmsTemplateRecord template);

    /// <summary>Replaces an existing template's editable fields; false when the code is unknown.</summary>
    bool TryUpdateTemplate(string code, SmsTemplateRecord updated);

    /// <summary>Removes a template; false when the code is unknown.</summary>
    bool TryDeleteTemplate(string code);
}
