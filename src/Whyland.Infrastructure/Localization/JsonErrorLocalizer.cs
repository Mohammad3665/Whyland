using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Whyland.Application.Common.Interfaces.Localization;
using Whyland.Shared.Errors;

namespace Whyland.Infrastructure.Localization;

/// <summary>
/// Resolves error text from the <c>*.localization.json</c> files embedded in this assembly.
/// <para>
/// Every file is a flat map of error code to message template; placeholders such as <c>{Max}</c> are filled
/// from the error's arguments, <c>{Field}</c> from the field it is bound to. The whole catalog is read once,
/// at startup, so a missing or duplicated key fails the build of the dictionary rather than a request.
/// </para>
/// </summary>
public sealed partial class JsonErrorLocalizer(ILogger<JsonErrorLocalizer> logger) : IErrorLocalizer
{
    /// <summary>Shown when a code has no entry; the detail stays in the log, never in the response.</summary>
    private const string FallbackMessage = "خطای نامشخص.";

    private const string ErrorsArgument = "Errors";

    private readonly ILogger<JsonErrorLocalizer> _logger = logger;

    private readonly IReadOnlyDictionary<string, string> _templates = LoadTemplates();

    /// <inheritdoc />
    public Error Localize(Error error)
    {
        if (error.Description is not null)
            return error;

        var localized = error with { Description = Resolve(error) };

        // An aggregated validation failure carries its per-field errors as an argument, and the page renders
        // those individually, so they are described too.
        if (localized.Arguments?.TryGetValue(ErrorsArgument, out var value) == true
            && value is IReadOnlyList<Error> nested)
        {
            localized = localized with
            {
                Arguments = new Dictionary<string, object?>(localized.Arguments)
                {
                    [ErrorsArgument] = nested.Select(Localize).ToList()
                }
            };
        }

        return localized;
    }

    private string Resolve(Error error)
    {
        if (!_templates.TryGetValue(error.Code, out var template))
        {
            _logger.LogWarning("No localized message is registered for error code {ErrorCode}.", error.Code);
            return FallbackMessage;
        }

        return Placeholder().Replace(template, match =>
        {
            var name = match.Groups[1].Value;

            var value = name.Equals("Field", StringComparison.OrdinalIgnoreCase)
                ? error.Field
                : error.Arguments is not null && error.Arguments.TryGetValue(name, out var argument)
                    ? argument
                    : null;

            return Format(value);
        });
    }

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };

    /// <summary>
    /// Reads every embedded <c>*.localization.json</c> into one map. A code defined twice is a bug that would
    /// otherwise pick a winner by file order, so it is raised instead of silently resolved.
    /// </summary>
    private static IReadOnlyDictionary<string, string> LoadTemplates()
    {
        var assembly = typeof(JsonErrorLocalizer).Assembly;

        var templates = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var resourceName in assembly.GetManifestResourceNames()
                     .Where(x => x.EndsWith(".localization.json", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(x => x, StringComparer.Ordinal))
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be opened.");

            var entries = JsonSerializer.Deserialize(stream, ErrorJsonContext.Default.DictionaryStringString)
                ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' is not a flat code/message map.");

            foreach (var (code, message) in entries)
            {
                if (templates.TryAdd(code, message))
                    continue;

                throw new InvalidOperationException(
                    $"Error code '{code}' is defined in more than one localization resource.");
            }
        }

        return templates;
    }

    [GeneratedRegex(@"\{([A-Za-z][A-Za-z0-9_]*)\}", RegexOptions.CultureInvariant)]
    private static partial Regex Placeholder();
}

/// <summary>Source-generated reader for the flat code/message maps, so startup does not reflect over JSON.</summary>
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class ErrorJsonContext : JsonSerializerContext;
