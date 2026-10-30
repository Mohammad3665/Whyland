using System.Collections.Concurrent;

namespace Whyland.Sms.Storage;

/// <summary>
/// Thread-safe in-memory <see cref="ISmsStore"/>. Traffic is bounded by
/// SmsService:MaxStoredMessages and the oldest entries are evicted first, so a long-running
/// development process cannot grow without limit.
/// </summary>
/// <param name="options">Simulator settings.</param>
public sealed class InMemorySmsStore(IOptions<SmsServiceOptions> options) : ISmsStore
{
    private readonly ConcurrentDictionary<string, SmsMessageRecord> _messages = new(StringComparer.Ordinal);
    private readonly ConcurrentQueue<string> _messageOrder = new();
    private readonly ConcurrentDictionary<Guid, SmsRequestRecord> _requests = new();
    private readonly ConcurrentQueue<Guid> _requestOrder = new();
    private readonly ConcurrentDictionary<string, SmsTemplateRecord> _templates = new(StringComparer.OrdinalIgnoreCase);

    public void AddMessage(SmsMessageRecord message)
    {
        _messages[message.MessageId] = message;
        _messageOrder.Enqueue(message.MessageId);
        Trim(_messages, _messageOrder, options.Value.MaxStoredMessages);
    }

    public void AddRequest(SmsRequestRecord request)
    {
        _requests[request.RequestId] = request;
        _requestOrder.Enqueue(request.RequestId);
        Trim(_requests, _requestOrder, options.Value.MaxStoredMessages);
    }

    public SmsMessageRecord? FindMessage(string messageId) =>
        _messages.GetValueOrDefault(messageId);

    public SmsRequestRecord? FindRequest(Guid requestId) =>
        _requests.GetValueOrDefault(requestId);

    public IReadOnlyList<SmsMessageRecord> ListMessages(
        string? recipient = null,
        string? status = null,
        string? templateCode = null,
        int limit = 100)
    {
        var query = _messages.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(recipient))
        {
            query = query.Where(m => m.Recipient.Contains(recipient, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(templateCode))
        {
            query = query.Where(m => m.TemplateCode?.Equals(templateCode, StringComparison.OrdinalIgnoreCase) == true);
        }

        return query
            .OrderByDescending(m => m.CreatedAt)
            .Take(ClampLimit(limit))
            .ToList();
    }

    public IReadOnlyList<SmsRequestRecord> ListRequests(
        string? recipient = null,
        string? status = null,
        int limit = 100)
    {
        var query = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(recipient))
        {
            query = query.Where(r => r.Recipient?.Contains(recipient, StringComparison.Ordinal) == true);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(r => r.ReceivedAt)
            .Take(ClampLimit(limit))
            .ToList();
    }

    public SmsTemplateRecord? FindTemplate(string code) => _templates.GetValueOrDefault(code);

    public IReadOnlyList<SmsTemplateRecord> ListTemplates() =>
        _templates.Values.OrderBy(t => t.Code, StringComparer.OrdinalIgnoreCase).ToList();

    public bool TryAddTemplate(SmsTemplateRecord template) =>
        _templates.TryAdd(template.Code, template);

    public bool TryUpdateTemplate(string code, SmsTemplateRecord updated)
    {
        if (!_templates.TryGetValue(code, out var existing))
        {
            return false;
        }

        existing.Name = updated.Name;
        existing.Content = updated.Content;
        existing.Parameters = updated.Parameters;
        existing.Description = updated.Description;
        existing.IsActive = updated.IsActive;
        existing.UpdatedAt = updated.UpdatedAt;
        return true;
    }

    public bool TryDeleteTemplate(string code) => _templates.TryRemove(code, out _);

    private static int ClampLimit(int limit) => Math.Clamp(limit, 1, 1_000);

    private static void Trim<TKey, TValue>(
        ConcurrentDictionary<TKey, TValue> items,
        ConcurrentQueue<TKey> order,
        int capacity)
        where TKey : notnull
    {
        while (items.Count > capacity && order.TryDequeue(out var oldest))
        {
            items.TryRemove(oldest, out _);
        }
    }
}
