namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The admin-panel section a site setting is managed from (s4.11).
/// </summary>
public enum SettingGroup
{
    /// <summary>
    /// General site configuration (titles, descriptions, ...).
    /// </summary>
    General = 0,

    /// <summary>
    /// Search-engine related settings.
    /// </summary>
    SEO = 1,

    /// <summary>
    /// Public contact information settings.
    /// </summary>
    Contact = 2,
}
