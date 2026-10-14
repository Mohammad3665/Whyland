using System;

namespace Whyland.Web.Areas.Admin.Models;

public class BreadcrumbItem
{
    public string Title { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool IsActive { get; set; }
}
