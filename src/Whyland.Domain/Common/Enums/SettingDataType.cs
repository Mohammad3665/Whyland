namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The logical storage type of a site-setting value, used to validate and
/// interpret the stored string (s4.11).
/// </summary>
public enum SettingDataType
{
    /// <summary>
    /// A free-form text value.
    /// </summary>
    String = 0,

    /// <summary>
    /// An integral numeric value.
    /// </summary>
    Integer = 1,

    /// <summary>
    /// A true/false value.
    /// </summary>
    Boolean = 2,

    /// <summary>
    /// A decimal (money-compatible) numeric value.
    /// </summary>
    Decimal = 3,

    /// <summary>
    /// A structured JSON document.
    /// </summary>
    Json = 4,
}
