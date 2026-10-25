using Microsoft.AspNetCore.Mvc;
using Whyland.Sms.Storage;
using Whyland.Sms.ViewModels;

namespace Whyland.Sms.Controllers;

/// <summary>
/// The delivered-message table: what the simulator accepted, for whom, and the exact text.
/// </summary>
/// <param name="store">In-memory message store.</param>
public class MessagesController(ISmsStore store) : PanelController
{
    /// <summary>Lists messages, newest first, with the same filters the API exposes.</summary>
    public IActionResult Index(string? recipient, string? status, int? limit)
    {
        var take = Math.Clamp(limit ?? PanelLimits.DefaultListSize, 1, PanelLimits.MaxListSize);

        return View(new MessageListViewModel(
            store.ListMessages(recipient, status, null, take),
            recipient,
            status,
            take));
    }

    /// <summary>Shows one message with its failure detail, if any.</summary>
    public IActionResult Details(string id) =>
        store.FindMessage(id) is { } message ? View(message) : NotFound();
}
