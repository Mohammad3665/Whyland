using System.Text.RegularExpressions;

namespace Whyland.Sms.Templates;

/// <summary>
/// Create/edit rules for template definitions. Both the versioned API and the dashboard
/// go through here, so the two surfaces can never drift into accepting different bodies.
/// </summary>
public static class SmsTemplateEditor
{
    /// <summary>Template codes are short, URL-safe and compared case-insensitively.</summary>
    public const string CodePattern = "^[A-Za-z0-9-]{3,40}$";

    /// <summary>True when the code can be stored.</summary>
    public static bool IsAllowedCode(string? code) =>
        !string.IsNullOrWhiteSpace(code) && Regex.IsMatch(code, CodePattern, RegexOptions.CultureInvariant);

    /// <summary>First reason the definition is unusable, or null when it can be stored.</summary>
    public static string? Validate(SaveSmsTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Content))
        {
            return "Name and Content are required.";
        }

        if (request.Parameters is null || request.Parameters.Count == 0)
        {
            return "At least one parameter must be declared.";
        }

        var bad = request.Parameters.FirstOrDefault(p =>
            string.IsNullOrWhiteSpace(p.Name)
            || !Regex.IsMatch(p.Name, "^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)
            || p.Type.ToLowerInvariant() is not ("string" or "integer"));

        if (bad is not null)
        {
            return $"Parameter '{bad.Name ?? "(empty)"}' needs a valid name and a type of string or integer.";
        }

        var declared = request.Parameters.Select(parameter => parameter.Name).ToHashSet(StringComparer.Ordinal);
        var undeclaredToken = SmsTemplateRenderer
            .TokensIn(request.Content)
            .FirstOrDefault(token => !declared.Contains(token));

        if (undeclaredToken is not null)
        {
            return $"Content uses {{{{{undeclaredToken}}}}} but no parameter '{undeclaredToken}' is declared.";
        }

        return null;
    }

    /// <summary>Builds the record to store; only requests that passed <see cref="Validate"/> reach this.</summary>
    public static SmsTemplateRecord Build(
        string code,
        SaveSmsTemplateRequest request,
        DateTime createdAt,
        DateTime updatedAt) =>
        new()
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = request.Name!.Trim(),
            Content = request.Content!,
            Description = request.Description,
            IsActive = request.IsActive ?? true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Parameters = request.Parameters!
                .ToDictionary(parameter => parameter.Name, StringComparer.Ordinal),
        };
}
