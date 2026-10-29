using Whyland.Shared.Text;

namespace Whyland.Application.Common.Validation;

/// <summary>
/// Turns any Persian or international phone spelling into the single stored form,
/// so a number is never matched, rate-limited or stored under two different strings (s5.1.4).
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>
    /// Returns the Latin digits of a typed number and drops everything else (spaces, dashes,
    /// brackets). The digit conversion itself lives in <see cref="Digits.ToLatin"/>, which every
    /// numeric input in the system goes through: a Unicode-aware digit test is not enough, because
    /// <c>char.IsDigit('۰')</c> is true, so a Persian number would otherwise be stored, compared
    /// and rate-limited as a different string from the same Latin number.
    /// </summary>
    /// <param name="value">Any text, typically a number typed by the user.</param>
    public static string ToLatinDigits(string? value)
        => new(Digits.ToLatin(value).Where(char.IsAsciiDigit).ToArray());

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
