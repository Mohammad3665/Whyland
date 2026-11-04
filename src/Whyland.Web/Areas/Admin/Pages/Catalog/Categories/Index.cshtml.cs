using Whyland.Application.Features.Admin.Categories.Commands.Create;
using Whyland.Application.Features.Admin.Categories.Commands.Delete;
using Whyland.Application.Features.Admin.Categories.Commands.Reorder;
using Whyland.Application.Features.Admin.Categories.Commands.Update;
using Whyland.Application.Features.Admin.Categories.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.Categories.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Users.Queries.GetOptions;
using Whyland.Shared.Errors;
using Whyland.Shared.Filters;
using Whyland.Shared.Results;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Categories;

/// <summary>
/// «دسته‌بندی‌ها» admin page (section 7.1 of the SRS): wired to the category feature
/// commands/queries through <see cref="IDispatcher"/>.
/// Rows are loaded through ?handler=Rows, the generic filter picker through ?handler=Fields and
/// the author pickers through ?handler=UserOptions; every mutation runs through the corresponding
/// command handler. The table itself is a client-side DataTable with RowReorder drag &amp; drop
/// ordering (see wwwroot/admin/js/categories.js), while the filter conditions are resolved by the
/// same shared data-table engine the users list uses.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Categories.View)]
public class CategoriesModel(
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
    /// Returns the categories in a single page that the client-side DataTable and its filters
    /// consume. The rows come back in tree order — a parent immediately followed by its children —
    /// which is the only order a drag &amp; drop list of siblings can be read in, and each row
    /// carries its parent, level and audit columns, so the edit and details modals need no extra call.
    /// </summary>
    /// <param name="filter">
    /// Paging, ordering and search parameters. The table itself does not page (it needs the whole
    /// tree in the DOM to reorder it), so an empty request is answered with one big page.
    /// </param>
    /// <param name="filters">
    /// Conditions built by the generic filter picker; ANDed together and resolved by the same
    /// shared engine the users list uses, so a filter means the same thing on both pages.
    /// </param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// The flat <c>DataTableResult&lt;CategoryListItemDto&gt;</c> envelope
    /// (draw / recordsTotal / recordsFiltered / data), or a 500 status
    /// with an error message.
    /// </returns>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter, List<FilterCondition>? filters, CancellationToken ct)
    {
        // One big page: the table paginates/searches/filters client-side, so only the row count
        // matters; the listing keeps the server-side tree order.
        if (string.IsNullOrWhiteSpace(filter.Length))
        {
            filter.Length = "10000";
            filter.Start = "0";
        }

        var result = await dispatcher.Send(
            new GetCategoriesForDataTableQuery(filter, filters), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the fields the table can be filtered or sorted on, with the operators each of them
    /// accepts, so the picker is built from the model instead of from markup.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFieldsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCategoryFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the accounts that hold the permission behind the requested author field, so the
    /// «ایجاد شده توسط» / «ویرایش شده توسط» filters list the people who could have done it — not
    /// every account — and are set by picking a name instead of pasting a guid.
    /// </summary>
    /// <param name="field">The filter field being filled, e.g. <c>CreatedBy</c>.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetUserOptionsAsync([FromQuery] string field, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetUserOptionsQuery(field), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    // Each write demands its own key on top of the view key the class carries: opening the list must
    // not be enough to change it. The check is made in code because an attribute on a page handler
    // method is ignored by Razor Pages (MVC1001).
    /// <summary>Creates a category from the create modal (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateCategoryCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Categories.Create)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an existing category from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the category id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateCategoryCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Categories.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes a category from the delete modal (?handler=Delete).</summary>
    /// <param name="id">The identifier of the category to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Categories.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteCategoryCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="rows">The reordered rows in visual order; only the ids matter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromBody] List<ReorderCategoryRow> rows, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Categories.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderCategoriesCommand(rows.Select(r => r.Id).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves visual order).</summary>
    /// <param name="Id">The category identifier.</param>
    /// <param name="DisplayOrder">The visual order displayed by the badge.</param>
    public sealed record ReorderCategoryRow(long Id, int DisplayOrder);

    /// <summary>
    /// Flattens any <see cref="BaseResult"/> (with or without data) into the
    /// JSON envelope the admin pages consume: 200 with <c>isSuccess=true</c>
    /// on success, 400 with the server-side error message (validation,
    /// not-found, ...) otherwise.
    /// </summary>
    /// <param name="result">The dispatched command outcome.</param>
    private static JsonResult ToJson(BaseResult result)
        => result.IsSuccess
            ? new JsonResult(new { isSuccess = true, message = (string?)null })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };

    /// <summary>
    /// Builds a readable message out of an <see cref="Error"/>: aggregated
    /// validation failures carry the individual errors in
    /// <c>Arguments["Errors"]</c>, everything else uses the plain description.
    /// </summary>
    /// <param name="error">The error to describe.</param>
    private static string MainMessage(Error error)
    {
        if (error.Arguments?.TryGetValue("Errors", out var value) == true &&
            value is IEnumerable<Error> items)
        {
            var messages = items.Select(e => e.Description);
            return string.Join(' ', messages);
        }

        return error.Description ?? "خطای نامشخص.";
    }
}
