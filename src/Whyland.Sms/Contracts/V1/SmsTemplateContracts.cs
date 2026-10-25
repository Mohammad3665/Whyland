using Whyland.Sms.Storage;

namespace Whyland.Sms.Contracts.V1;

/// <summary>
/// Public shape of an SMS template (s11.3.4.1).
/// </summary>
/// <param name="Code">Unique code referenced by send requests.</param>
/// <param name="Name">Display name.</param>
/// <param name="Content">Body with {{placeholder}} tokens.</param>
/// <param name="Parameters">Declared placeholders.</param>
/// <param name="Description">Optional curator note.</param>
/// <param name="IsActive">Inactive templates are refused when sending.</param>
/// <param name="CreatedAt">UTC creation stamp.</param>
/// <param name="UpdatedAt">UTC last-edit stamp.</param>
public sealed record SmsTemplateResponse(
    string Code,
    string Name,
    string Content,
    IReadOnlyList<SmsTemplateParameter> Parameters,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>Maps a stored template onto its API shape.</summary>
    public static SmsTemplateResponse From(SmsTemplateRecord template) =>
        new(template.Code, template.Name, template.Content, template.Parameters.Values.ToList(),
            template.Description, template.IsActive, template.CreatedAt, template.UpdatedAt);
}

/// <summary>
/// Body for creating (POST) or replacing (PUT) a template.
/// </summary>
/// <param name="Code">Template code; required on create, taken from the route on update.</param>
/// <param name="Name">Display name.</param>
/// <param name="Content">Body with {{placeholder}} tokens.</param>
/// <param name="Parameters">Placeholders the body may use.</param>
/// <param name="Description">Optional curator note.</param>
/// <param name="IsActive">Defaults to active when omitted.</param>
public sealed record SaveSmsTemplateRequest(
    string? Code,
    string? Name,
    string? Content,
    IReadOnlyList<SmsTemplateParameter>? Parameters,
    string? Description,
    bool? IsActive);
