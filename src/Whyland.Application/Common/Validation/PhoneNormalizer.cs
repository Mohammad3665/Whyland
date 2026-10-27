namespace Whyland.Application.Common.Validation;

/// <summary>
/// Turns any Persian or international phone spelling into the single stored form,
/// so a number is never matched, rate-limited or stored under two different strings (s5.1.4).
/// </summary>
public static class PhoneNormalizer
{
    /// <summary>Normalizes 0912…, +98912…, 98912… and 0098912… to 09xxxxxxxxx.</summary>
    /// <param name="phone">The raw value typed by the user.</param>
    public static string Normalize(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("0098")) return "0" + digits[4..];
        if (digits.StartsWith("98")) return "0" + digits[2..];

        return digits;
    }
}
