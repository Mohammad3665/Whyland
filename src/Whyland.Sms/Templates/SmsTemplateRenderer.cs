using System.Text.RegularExpressions;

namespace Whyland.Sms.Templates;

/// <summary>
/// Outcome of rendering a template: either the final text or the documented reason it
/// could not be produced (s11.3.4.2).
/// </summary>
/// <param name="IsSuccess">True when every placeholder was resolved.</param>
/// <param name="Content">Rendered message text on success.</param>
/// <param name="ErrorCode">One of <see cref="SmsErrorCodes"/> on failure.</param>
/// <param name="ErrorMessage">Reason a developer can act on.</param>
/// <param name="Field">Parameter name the failure refers to, when applicable.</param>
public sealed record RenderOutcome(
    bool IsSuccess,
    string? Content,
    string? ErrorCode,
    string? ErrorMessage,
    string? Field)
{
    /// <summary>A rendered body.</summary>
    public static RenderOutcome Success(string content) => new(true, content, null, null, null);

    /// <summary>A rejected render.</summary>
    public static RenderOutcome Failure(string errorCode, string message, string? field = null) =>
        new(false, null, errorCode, message, field);
}

/// <summary>
/// Replaces <c>{{name}}</c> tokens in a template body with validated values (s11.3.4.3).
/// </summary>
public sealed partial class SmsTemplateRenderer
{
    /// <summary>Substitutes every declared placeholder, rejecting missing or mistyped values.</summary>
    public RenderOutcome Render(
        SmsTemplateRecord template,
        IReadOnlyDictionary<string, JsonElement>? values)
    {
        var supplied = values is null
            ? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, JsonElement>(values, StringComparer.OrdinalIgnoreCase);

        var unknown = supplied.Keys
            .FirstOrDefault(key => !template.Parameters.ContainsKey(key));

        if (unknown is not null)
        {
            return RenderOutcome.Failure(
                SmsErrorCodes.UnknownTemplateParameter,
                $"Template '{template.Code}' does not declare a parameter named '{unknown}'.",
                unknown);
        }

        var resolved = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var parameter in template.Parameters.Values)
        {
            if (!supplied.TryGetValue(parameter.Name, out var value))
            {
                if (parameter.DefaultValue is not null)
                {
                    resolved[parameter.Name] = parameter.DefaultValue;
                    continue;
                }

                if (parameter.Required)
                {
                    return RenderOutcome.Failure(
                        SmsErrorCodes.TemplateParameterMissing,
                        $"Template '{template.Code}' requires parameter '{parameter.Name}'.",
                        parameter.Name);
                }

                resolved[parameter.Name] = string.Empty;
                continue;
            }

            if (!TryReadValue(value, parameter.Type, out var text))
            {
                return RenderOutcome.Failure(
                    SmsErrorCodes.TemplateParameterInvalid,
                    $"Parameter '{parameter.Name}' must be of type '{parameter.Type}'.",
                    parameter.Name);
            }

            resolved[parameter.Name] = text;
        }

        var undeclared = TokenPattern()
            .Matches(template.Content)
            .Select(match => match.Groups["name"].Value)
            .FirstOrDefault(name => !template.Parameters.ContainsKey(name));

        if (undeclared is not null)
        {
            return RenderOutcome.Failure(
                SmsErrorCodes.InvalidTemplate,
                $"Template '{template.Code}' uses {{{{{undeclared}}}}} but does not declare that parameter.",
                undeclared);
        }

        var content = TokenPattern().Replace(
            template.Content,
            match => resolved.TryGetValue(match.Groups["name"].Value, out var value) ? value : match.Value);

        return RenderOutcome.Success(content);
    }

    private static bool TryReadValue(JsonElement value, string type, out string text)
    {
        switch (type.ToLowerInvariant())
        {
            case "integer":
                if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
                {
                    text = number.ToString();
                    return true;
                }

                if (value.ValueKind == JsonValueKind.String
                    && long.TryParse(value.GetString(), out var parsed))
                {
                    text = parsed.ToString();
                    return true;
                }

                text = string.Empty;
                return false;

            case "string":
                text = value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString() ?? string.Empty,
                    JsonValueKind.Number => value.GetRawText(),
                    JsonValueKind.True => "true",
                    JsonValueKind.False => "false",
                    _ => string.Empty,
                };

                return value.ValueKind is JsonValueKind.String or JsonValueKind.Number
                    or JsonValueKind.True or JsonValueKind.False;

            default:
                text = string.Empty;
                return false;
        }
    }

    [GeneratedRegex(@"\{\{\s*(?<name>[A-Za-z][A-Za-z0-9_]*)\s*\}\}", RegexOptions.CultureInvariant)]
    internal static partial Regex TokenPattern();

    /// <summary>Lists the distinct placeholder names a template body uses.</summary>
    public static IReadOnlyList<string> TokensIn(string content) =>
        TokenPattern().Matches(content)
            .Select(match => match.Groups["name"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();
}
