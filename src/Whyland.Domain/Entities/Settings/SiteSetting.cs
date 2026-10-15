
namespace Whyland.Domain.Entities.Settings;

/// <summary>
/// An editable site configuration entry stored as a key/value pair with metadata (s4.11).
/// </summary>
public class SiteSetting : BaseEntity<long>
{
    #region Properties

    /// <summary>
    /// Unique lookup key of the setting (e.g. "site.title").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The stored value, to be interpreted according to <see cref="DataType"/>.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The section of the admin panel the setting is managed from.
    /// </summary>
    public SettingGroup Group { get; set; }

    /// <summary>
    /// The logical type of <see cref="Value"/> (string, boolean, number, ...).
    /// </summary>
    public SettingDataType DataType { get; set; }

    /// <summary>
    /// Public settings are readable by the anonymous site, others only by managers.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Human-readable explanation shown next to the setting in the admin panel.
    /// </summary>
    public string? Description { get; set; }

    #endregion
}
