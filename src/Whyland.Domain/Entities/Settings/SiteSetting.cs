namespace Whyland.Domain.Entities.Settings;

/// <summary>
/// An editable site configuration entry stored as a key/value pair with metadata (s4.11).
/// </summary>
public class SiteSetting : BaseEntity<long>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Description"/>; also the database column size.</summary>
    public const int MaxDescriptionLength = 500;

    /// <summary>Maximum allowed length of <see cref="Key"/>; also the database column size.</summary>
    public const int MaxKeyLength = 200;

    /// <summary>Maximum allowed length of <see cref="Value"/>; also the database column size.</summary>
    public const int MaxValueLength = 2000;

    #endregion

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
