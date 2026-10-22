
namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The type of discount that can be applied to a course (s7.2.5).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DiscountType
{
    /// <summary>
    /// No discount is applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// A percentage discount; value must satisfy 0 &lt;= value &lt;= 100.
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// A fixed-amount discount; must never make the final price negative.
    /// </summary>
    FixedAmount = 2
}
