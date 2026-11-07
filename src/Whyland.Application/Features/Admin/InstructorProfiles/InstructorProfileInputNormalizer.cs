namespace Whyland.Application.Features.Admin.InstructorProfiles;

/// <summary>
/// Normalizes the instructor form values into the shape the aggregate stores: trimmed text,
/// Latin digits for banking fields, and absolute http(s) URLs for social links.
/// </summary>
internal static class InstructorProfileInputNormalizer
{
    /// <summary>Trims a value and turns whitespace into null.</summary>
    public static string? Text(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// Converts Persian/Arabic digits to Latin, removes spaces, dashes and an optional IR prefix,
    /// then returns null when nothing is left.
    /// </summary>
    public static string? Digits(string? value, bool stripIrPrefix = false)
    {
        var normalized = Whyland.Shared.Text.Digits.ToLatin(value)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Trim();

        if (stripIrPrefix && normalized.StartsWith("IR", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[2..];

        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    /// <summary>Adds https:// when the user typed a host without a scheme.</summary>
    public static string? Url(string? value)
    {
        var trimmed = Text(value);
        if (trimmed is null)
            return null;

        return trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : "https://" + trimmed.TrimStart('/');
    }

    /// <summary>True when the optional value is an absolute http(s) URL.</summary>
    public static bool IsHttpUrl(string? value)
    {
        var normalized = Url(value);
        return normalized is null ||
               (Uri.TryCreate(normalized, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps));
    }

    /// <summary>True when the optional value, after digit normalization, has exactly the given length.</summary>
    public static bool IsDigitsOfLength(string? value, int length, bool stripIrPrefix = false)
    {
        var normalized = Digits(value, stripIrPrefix);
        return normalized is null || (normalized.Length == length && normalized.All(char.IsDigit));
    }

    /// <summary>True when the optional value, after digit normalization, is a digit string in the range.</summary>
    public static bool IsDigitsBetween(string? value, int min, int max)
    {
        var normalized = Digits(value);
        return normalized is null ||
               (normalized.Length >= min && normalized.Length <= max && normalized.All(char.IsDigit));
    }
}
