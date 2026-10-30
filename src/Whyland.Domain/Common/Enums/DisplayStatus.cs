namespace Whyland.Domain.Common.Enums;

/// <summary>
/// The public visibility status of content such as courses and blog posts.
/// Only published content may be publicly discoverable (s6.7, s7.2.7).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisplayStatus
{
    /// <summary>
    /// Not publicly exposed; under preparation by the administrator.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Publicly discoverable through regular listings.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Withdrawn from public exposure without being draft.
    /// </summary>
    Hidden = 2
}
