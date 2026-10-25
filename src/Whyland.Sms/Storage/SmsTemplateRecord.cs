namespace Whyland.Sms.Storage;

/// <summary>
/// Definition of one replaceable placeholder inside an SMS template (s11.3.4.2).
/// </summary>
/// <param name="Name">Placeholder name as written in the template body.</param>
/// <param name="Type">Declared value type: string or integer.</param>
/// <param name="Required">Whether callers must supply a value.</param>
/// <param name="DefaultValue">Value used when an optional parameter is omitted.</param>
/// <param name="Description">Explanation shown to whoever curates the template.</param>
public sealed record SmsTemplateParameter(
    string Name,
    string Type,
    bool Required = true,
    string? DefaultValue = null,
    string? Description = null);

/// <summary>
/// A reusable SMS body with named placeholders. Templates are the one editable
/// dataset of the simulator, so they are mutable through the dashboard/API (s11.3.4.1).
/// </summary>
public sealed class SmsTemplateRecord
{
    /// <summary>Unique, case-insensitive code referenced by send requests.</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Body containing {{placeholder}} tokens.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Declared placeholders, keyed by name.</summary>
    public Dictionary<string, SmsTemplateParameter> Parameters { get; set; } =
        new(StringComparer.Ordinal);

    /// <summary>Optional note for the template curator.</summary>
    public string? Description { get; set; }

    /// <summary>Inactive templates are refused at send time.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>UTC creation stamp.</summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>UTC stamp of the last edit.</summary>
    public DateTime UpdatedAt { get; set; }
}
