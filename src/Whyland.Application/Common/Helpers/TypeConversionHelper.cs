using System.ComponentModel;
using System.Globalization;

namespace Whyland.Application.Common.Helpers;

/// <summary>
/// Converts string filter values coming from the client to the target property type
/// safely, using invariant culture. Unconvertible values report failure so the caller
/// can skip the filter condition instead of throwing.
/// </summary>
public static class TypeConversionHelper
{
    /// <summary>
    /// Attempts to convert the provided string value to the target type.
    /// </summary>
    /// <param name="value">The raw string value (e.g. from a filter request).</param>
    /// <param name="targetType">The property type to convert to; nullable types are unwrapped.</param>
    /// <param name="converted">The converted value when the method returns true.</param>
    /// <returns>true if conversion succeeded; otherwise false.</returns>
    public static bool TryConvert(string? value, Type targetType, out object? converted)
    {
        converted = null;

        if (value is null)
            return false;

        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (nonNullableType.IsEnum)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                Enum.TryParse(nonNullableType, value.Trim(), ignoreCase: true, out var enumValue))
            {
                converted = enumValue;
                return true;
            }

            return false;
        }

        try
        {
            var culture = CultureInfo.InvariantCulture;

            object? result = nonNullableType switch
            {
                _ when nonNullableType == typeof(string) => value,
                _ when nonNullableType == typeof(bool) =>
                    value.Trim() is "1" or "true" or "True"
                        ? true
                        : value.Trim() is "0" or "false" or "False"
                            ? false
                            : null,
                _ when nonNullableType == typeof(Guid) => Guid.Parse(value.Trim()),
                _ when nonNullableType == typeof(DateOnly) => DateOnly.Parse(value, culture),
                _ when nonNullableType == typeof(TimeOnly) => TimeOnly.Parse(value, culture),
                _ => TypeDescriptor.GetConverter(nonNullableType)
                    .ConvertFromString(null, culture, value.Trim())
            };

            if (result is null)
                return false;

            converted = result;
            return true;
        }
        catch (Exception ex) when (ex is FormatException
                                   or InvalidCastException
                                   or OverflowException
                                   or ArgumentException
                                   or NotSupportedException)
        {
            return false;
        }
    }
}
