using Microsoft.AspNetCore.Mvc;
using Whyland.Sms.Contracts.V1;
using Whyland.Sms.Storage;
using Whyland.Sms.ViewModels;

namespace Whyland.Sms.Controllers;

/// <summary>
/// Landing page: counters over the stored window plus the newest traffic, so an operator can
/// see whether sends are landing before drilling into a table.
/// </summary>
/// <param name="store">In-memory traffic store.</param>
public class DashboardController(ISmsStore store) : PanelController
{
    /// <summary>Builds the counters and recent lists.</summary>
    public IActionResult Index()
    {
        var messages = store.ListMessages(limit: PanelLimits.Summary);
        var requests = store.ListRequests(limit: PanelLimits.Summary);

        return View(new DashboardViewModel(
            MessageCount: messages.Count,
            RequestCount: requests.Count,
            FailedRequestCount: requests.Count(request =>
                request.Status.Equals(SmsStatus.Failed, StringComparison.OrdinalIgnoreCase)),
            TemplateCount: store.ListTemplates().Count,
            Window: messages.Count + requests.Count,
            RecentMessages: messages.Take(8).ToList(),
            RecentFailures: requests
                .Where(request => request.Status.Equals(SmsStatus.Failed, StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList()));
    }
}
