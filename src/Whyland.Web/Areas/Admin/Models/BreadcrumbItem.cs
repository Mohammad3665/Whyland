namespace Whyland.Web.Areas.Admin.Models;

/// <summary>
/// A single breadcrumb node rendered by the admin breadcrumb trail.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>Display text of the breadcrumb node.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Navigation target of the node; null renders it as the current (non-link) step.</summary>
    public string? Url { get; set; }

    /// <summary>Whether this node is the current page (rendered as active).</summary>
    public bool IsActive { get; set; }
}
