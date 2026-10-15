
namespace Whyland.Domain.Common.Enums;

/// <summary>
/// Optional gender information collected in the user profile (s5.1.2).
/// Only information strictly necessary for business requirements must be collected.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    /// <summary>
    /// Not provided.
    /// </summary>
    NotSpecified = 0,

    Male = 1,

    Female = 2
}
