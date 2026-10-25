using Microsoft.AspNetCore.Mvc;
using Whyland.Sms.Storage;
using Whyland.Sms.ViewModels;

namespace Whyland.Sms.Controllers;

/// <summary>
/// The inbound-call audit table. Every API call is recorded here, including the rejected ones,
/// which is what makes a failed OTP request explainable after the fact.
/// </summary>
/// <param name="store">In-memory request store.</param>
public class RequestsController(ISmsStore store) : PanelController
{
    /// <summary>Lists requests, newest first.</summary>
    public IActionResult Index(string? recipient, string? status, int? limit)
    {
        var take = Math.Clamp(limit ?? PanelLimits.DefaultListSize, 1, PanelLimits.MaxListSize);

        return View(new RequestListViewModel(
            store.ListRequests(recipient, status, take),
            recipient,
            status,
            take));
    }

    /// <summary>Shows one request with its raw payload.</summary>
    public IActionResult Details(Guid id) =>
        store.FindRequest(id) is { } request ? View(request) : NotFound();
}
