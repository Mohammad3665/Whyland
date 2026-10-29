namespace Whyland.Shared.Text;

/// <summary>
/// Converts the digits a Persian or Arabic keyboard produces into the Latin digits the system
/// stores and compares. Applied to every numeric input, because a number is the same number
/// whichever alphabet it was typed in — and because an unconverted ۰ breaks a digit test
/// (<c>char.IsDigit('۰')</c> is true), a numeric parse, a hash comparison and a uniqueness
/// lookup all at once.
/// </summary>
public static class Digits
{
    private const string Persian = "۰۱۲۳۴۵۶۷۸۹";

    private const string ArabicIndic = "٠١٢٣٤٥٦٧٨٩";

    /// <summary>
    /// Returns <paramref name="value"/> with Persian and Arabic-Indic digits replaced by their
    /// Latin equivalents; every other character is left exactly as it was.
    /// </summary>
    /// <param name="value">Any text; null becomes an empty string.</param>
    public static string ToLatin(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (!ContainsEasternDigits(value))
            return value;

        var chars = value.ToCharArray();

        for (var i = 0; i < chars.Length; i++)
        {
            var digit = Persian.IndexOf(chars[i]);
            if (digit < 0)
                digit = ArabicIndic.IndexOf(chars[i]);

            if (digit >= 0)
                chars[i] = (char)('0' + digit);
        }

        return new string(chars);
    }

    /// <summary>True when the text holds at least one Persian or Arabic-Indic digit.</summary>
    public static bool ContainsEasternDigits(string value)
        => value.AsSpan().IndexOfAny(Persian + ArabicIndic) >= 0;

    /// <summary>
    /// Converts a written-out number into the form a numeric parser understands: Latin digits,
    /// the Latin decimal separator and no thousands separator. Persian writing uses ٫ for the
    /// decimal point and ٬ for thousands, so ۱۲۳۴۵۶٫۷۸ becomes 123456.78.
    /// </summary>
    /// <param name="value">Any text; null becomes an empty string.</param>
    public static string ToLatinNumber(string? value)
    {
        var latin = ToLatin(value);
        if (latin.Length == 0)
            return latin;

        return latin.Replace('٫', '.').Replace("٬", string.Empty);
    }
}
