using Whyland.Sms.Contracts.V1;
using Whyland.Sms.Storage;

namespace Whyland.Sms.ViewModels;

/// <summary>Row count the dashboard reads when it summarizes traffic.</summary>
public static class PanelLimits
{
    /// <summary>Rows behind the dashboard counters and the recent-activity lists.</summary>
    public const int Summary = 200;

    /// <summary>Default page size for the message and request tables.</summary>
    public const int DefaultListSize = 100;

    /// <summary>Highest page size the store will hand back.</summary>
    public const int MaxListSize = 1_000;
}

/// <summary>Credentials for the dashboard sign-in form.</summary>
public sealed class LoginPanelViewModel
{
    /// <summary>Configured dashboard user name.</summary>
    public string? Username { get; set; }

    /// <summary>Submitted password; never persisted.</summary>
    public string? Password { get; set; }

    /// <summary>Keeps the cookie alive for the full session length instead of the browser session.</summary>
    public bool RememberMe { get; set; }

    /// <summary>False when the service has no dashboard password configured, so the form explains itself.</summary>
    public bool CredentialsConfigured { get; set; } = true;
}

/// <summary>Filters applied to the message table.</summary>
/// <param name="Messages">Rows newest-first.</param>
public sealed record MessageListViewModel(
    IReadOnlyList<SmsMessageRecord> Messages,
    string? Recipient,
    string? Status,
    int Limit);

/// <summary>Filters applied to the request table.</summary>
/// <param name="Requests">Rows newest-first.</param>
public sealed record RequestListViewModel(
    IReadOnlyList<SmsRequestRecord> Requests,
    string? Recipient,
    string? Status,
    int Limit);

/// <summary>Counters shown on the dashboard home.</summary>
/// <param name="Window">How many recent rows the counters were computed from.</param>
/// <param name="RecentFailures">
/// Rejected API calls. A rejected send never produces a message row, so failures can only be
/// read from the request log.
/// </param>
public sealed record DashboardViewModel(
    int MessageCount,
    int RequestCount,
    int FailedRequestCount,
    int TemplateCount,
    int Window,
    IReadOnlyList<SmsMessageRecord> RecentMessages,
    IReadOnlyList<SmsRequestRecord> RecentFailures);

/// <summary>Create/edit form for one template.</summary>
public sealed class TemplateFormViewModel
{
    /// <summary>False on the create form, where the code is editable.</summary>
    public bool IsNew { get; set; }

    /// <summary>Template code; route-bound on edit.</summary>
    public string? Code { get; set; }

    /// <summary>Display name.</summary>
    public string? Name { get; set; }

    /// <summary>Body with {{placeholder}} tokens.</summary>
    public string? Content { get; set; }

    /// <summary>Curator note.</summary>
    public string? Description { get; set; }

    /// <summary>Inactive templates are refused at send time.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Declared placeholders.</summary>
    public List<TemplateParameterFormModel> Parameters { get; set; } = [];

    /// <summary>Renders the stored record into the form.</summary>
    public static TemplateFormViewModel From(SmsTemplateRecord template) =>
        new()
        {
            IsNew = false,
            Code = template.Code,
            Name = template.Name,
            Content = template.Content,
            Description = template.Description,
            IsActive = template.IsActive,
            Parameters = template.Parameters.Values
                .Select(p => new TemplateParameterFormModel
                {
                    Name = p.Name,
                    Type = p.Type,
                    Required = p.Required,
                    DefaultValue = p.DefaultValue,
                    Description = p.Description,
                })
                .ToList(),
        };

    /// <summary>Maps the submitted form onto the same contract the API uses.</summary>
    public SaveSmsTemplateRequest ToRequest() =>
        new(
            Code,
            Name,
            Content,
            Parameters
                .Where(p => !string.IsNullOrWhiteSpace(p.Name))
                .Select(p => new SmsTemplateParameter(
                    p.Name!.Trim(),
                    string.IsNullOrWhiteSpace(p.Type) ? "string" : p.Type.Trim().ToLowerInvariant(),
                    p.Required,
                    string.IsNullOrWhiteSpace(p.DefaultValue) ? null : p.DefaultValue.Trim(),
                    p.Description))
                .ToList(),
            Description,
            IsActive);
}

/// <summary>One declared placeholder inside the form.</summary>
public sealed class TemplateParameterFormModel
{
    /// <summary>Placeholder name as written between the braces.</summary>
    public string? Name { get; set; }

    /// <summary>string or integer.</summary>
    public string? Type { get; set; } = "string";

    /// <summary>Whether callers must supply it.</summary>
    public bool Required { get; set; } = true;

    /// <summary>Fallback used when the parameter is optional and omitted.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Explanation for the curator.</summary>
    public string? Description { get; set; }
}
