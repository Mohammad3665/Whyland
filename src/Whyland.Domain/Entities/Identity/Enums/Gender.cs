namespace Whyland.Domain.Entities.Identity.Enums;

/// <summary>
/// Optional gender information collected in the user profile (s5.1.2).
/// Only information strictly necessary for business requirements must be collected.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Gender
{
    /// <summary>
    /// Male.
    /// </summary>
    Male = 0,

    /// <summary>
    /// Female.
    /// </summary>
    Female = 1
}
