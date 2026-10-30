namespace Whyland.Sms.Controllers;

/// <summary>
/// Base for every dashboard controller. The policy pins these pages to the panel cookie,
/// so a leaked API key alone cannot browse stored messages and vice versa.
/// </summary>
[Authorize(Policy = PanelAuthentication.Policy)]
public abstract class PanelController : Controller
{
    /// <summary>Name shown in the dashboard header.</summary>
    protected string SignedInUser => User.Identity?.Name ?? "پنل";
}
