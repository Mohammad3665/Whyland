namespace Whyland.Application.Common.Validation;

/// <summary>
/// Turns any Persian or international phone spelling into the single stored form,
/// so a number is never matched, rate-limited or stored under two different strings (s5.1.4).
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>Persian (۰-۹) and Arabic-Indic (٠-٩) digits, in ascending order from zero.</summary>
    private const string PersianDigits = "۰۱۲۳۴۵۶۷۸۹";

    private const string ArabicIndicDigits = "٠١٢٣٤٥٦٧٨٩";

    /// <summary>
    /// Replaces Persian and Arabic-Indic digits with their Latin equivalents and drops the
    /// separator characters people type between them. Unicode-aware digit tests are not enough:
    /// <c>char.IsDigit('۰')</c> is true, so a Persian number would survive every later step and
    /// be stored, compared and rate-limited as a different string from the same Latin number.
    /// </summary>
    /// <param name="value">Any text, typically a number typed by the user.</param>
    public static string ToLatinDigits(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var chars = new char[value.Length];
        var length = 0;

        foreach (var character in value)
        {
            var digit = PersianDigits.IndexOf(character);
            if (digit < 0)
                digit = ArabicIndicDigits.IndexOf(character);

            if (digit >= 0)
            {
                chars[length++] = (char)('0' + digit);
                continue;
            }

            if (char.IsAsciiDigit(character))
                chars[length++] = character;
        }

        return new string(chars, 0, length);
    }

    /// <summary>Normalizes 0912…, +98912…, 98912… and 0098912… to 09xxxxxxxxx.</summary>
    /// <param name="phone">The raw value typed by the user.</param>
    public static string Normalize(string phone)
    {
        var digits = ToLatinDigits(phone);

        if (digits.StartsWith("0098")) return "0" + digits[4..];
        if (digits.StartsWith("98")) return "0" + digits[2..];

        return digits;
    }

    /// <summary>
    /// Tells whether a typed number has the shape of a real one: a country prefix and 10 to 15
    /// digits, written in Persian or in Latin digits. Validators use it so a Persian number is
    /// judged by the same rule as its Latin equivalent instead of failing on the digits alone.
    /// </summary>
    /// <param name="phone">The raw value typed by the user; null or empty counts as absent.</param>
    public static bool HasValidShape(string? phone)
    {
        var digits = ToLatinDigits(phone);
        return digits.Length is >= 10 and <= 15;
    }
}
