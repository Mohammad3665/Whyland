namespace Whyland.Sms.Provider.Contracts;

/// <summary>
/// Wire contract for <c>POST /api/v1/sms/send</c> (v1). Mirrors the published SMS API
/// document; deliberately duplicated instead of referenced so the package stays usable
/// without the service project (s11.3.8).
/// </summary>
/// <param name="To">Recipient number.</param>
/// <param name="Message">Raw text; mutually exclusive with TemplateCode.</param>
/// <param name="TemplateCode">Template code; used together with Parameters.</param>
/// <param name="Parameters">Values substituted into the template.</param>
public sealed record SmsSendApiRequest(
    string To,
    string? Message = null,
    string? TemplateCode = null,
    Dictionary<string, object?>? Parameters = null);

/// <summary>
/// Wire response of the SMS API. Carries either a message id or a documented error code.
/// </summary>
/// <param name="Success">True when the service recorded the message.</param>
/// <param name="MessageId">Identifier of the recorded message.</param>
/// <param name="Status">Pending / Sent / Failed.</param>
/// <param name="ErrorCode">Stable machine-readable code, e.g. Sms.TemplateNotFound.</param>
/// <param name="ErrorMessage">Developer-readable reason.</param>
public sealed record SmsSendApiResponse(
    bool Success,
    string? MessageId,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

/// <summary>
/// Wire contract of one stored template, used for diagnostics and template-aware callers.
/// </summary>
/// <param name="Code">Template code.</param>
/// <param name="Name">Display name.</param>
/// <param name="Content">Body with {{placeholder}} tokens.</param>
/// <param name="Parameters">Declared placeholders.</param>
/// <param name="Description">Optional curator note.</param>
/// <param name="IsActive">Whether the template may be used.</param>
/// <param name="CreatedAt">UTC creation stamp.</param>
/// <param name="UpdatedAt">UTC last-edit stamp.</param>
public sealed record SmsTemplateApiResponse(
    string Code,
    string Name,
    string Content,
    IReadOnlyList<SmsTemplateParameterResponse> Parameters,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Wire contract of one template placeholder definition.
/// </summary>
/// <param name="Name">Placeholder name.</param>
/// <param name="Type">string or integer.</param>
/// <param name="Required">Whether callers must supply it.</param>
/// <param name="DefaultValue">Fallback for optional parameters.</param>
/// <param name="Description">Explanation of the parameter.</param>
public sealed record SmsTemplateParameterResponse(
    string Name,
    string Type,
    bool Required,
    string? DefaultValue,
    string? Description);
