using Whyland.Application.Features.Admin.Faqs.Commands.Create;
using Whyland.Application.Features.Admin.Faqs.Commands.Delete;
using Whyland.Application.Features.Admin.Faqs.Commands.Reorder;
using Whyland.Application.Features.Admin.Faqs.Commands.Update;
using Whyland.Application.Features.Admin.Faqs.Queries.GetForDataTable;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Content.Faqs;

/// <summary>
/// «سوالات متداول» admin page (sections 11.1 / 11.2 of the UI/UX doc): wired to
/// the FAQ feature commands/queries through <see cref="IDispatcher"/>.
/// Rows are loaded through ?handler=Rows and every mutation runs through the
/// corresponding command handler; the table itself is a client-side DataTable
/// with RowReorder drag &amp; drop ordering (see wwwroot/admin/js/faqs.js).
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Faqs.View)]
public class FaqsModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>
    /// Renders the page scaffold; the table body is filled client-side through
    /// <see cref="OnGetRowsAsync"/>.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Returns all FAQs in a single page (ordered by DisplayOrder) that the
    /// client-side DataTable and its filters consume.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// The flat <c>DataTableResult&lt;FaqListItemDto&gt;</c> envelope
    /// (draw / recordsTotal / recordsFiltered / data), or a 500 status
    /// with an error message.
    /// </returns>
    public async Task<IActionResult> OnGetRowsAsync(CancellationToken ct)
    {
        // One big page: the table paginates/searches/filters client-side, so only
        // the row count matters; ordering stays on DisplayOrder for drag & drop.
        var result = await dispatcher.Send(new GetFaqsForDataTableQuery(
            new DataTableFilter
            {
                Length = "10000",
                Start = "0",
                SortColumn = "displayorder",
                SortColumnDirection = "asc",
            }), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    // Each write demands its own key on top of the view key the class carries: opening the list must
    // not be enough to change it. The check is made in code because an attribute on a page handler
    // method is ignored by Razor Pages (MVC1001).
    /// <summary>Creates a FAQ from the create modal (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Faqs.Create)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an existing FAQ from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the FAQ id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Faqs.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes a FAQ from the delete modal (?handler=Delete).</summary>
    /// <param name="id">The identifier of the FAQ to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Faqs.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteFaqCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="rows">The reordered rows in visual order; only the ids matter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromBody] List<ReorderFaqRow> rows, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Faqs.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderFaqsCommand(rows.Select(r => r.Id).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves visual order).</summary>
    /// <param name="Id">The FAQ identifier.</param>
    /// <param name="DisplayOrder">The visual order displayed by the badge.</param>
    public sealed record ReorderFaqRow(long Id, int DisplayOrder);
}
