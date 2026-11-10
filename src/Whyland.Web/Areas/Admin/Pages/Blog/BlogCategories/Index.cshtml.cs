using Whyland.Application.Features.Admin.BlogCategories.Commands.Create;
using Whyland.Application.Features.Admin.BlogCategories.Commands.Delete;
using Whyland.Application.Features.Admin.BlogCategories.Commands.Reorder;
using Whyland.Application.Features.Admin.BlogCategories.Commands.Update;
using Whyland.Application.Features.Admin.BlogCategories.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.BlogCategories.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Users.Queries.GetOptions;
using Whyland.Domain.Model.Blog.BlogCategory.Identifiers;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Blog.BlogCategories;

/// <summary>
/// «دسته‌بندی‌های مقالات» admin page (section 12.1.1 of the SRS): wired to the blog category feature
/// commands/queries through <see cref="IDispatcher"/>.
/// Rows are loaded through ?handler=Rows, the generic filter picker through ?handler=Fields and
/// the author pickers through ?handler=UserOptions; every mutation runs through the corresponding
/// command handler. The table itself is a client-side DataTable with RowReorder drag &amp; drop
/// ordering (see wwwroot/admin/js/blog-categories.js), while the filter conditions are resolved by the
/// same shared data-table engine the course categories and the users list use.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.BlogCategories.View)]
public class BlogCategoriesModel(
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
    /// Returns the blog categories in a single page that the client-side DataTable and its filters
    /// consume. The rows come back in tree order — a parent immediately followed by its children —
    /// which is the only order a drag &amp; drop list of siblings can be read in, and each row
    /// carries its parent, level, post count and audit columns, so the edit and details modals need
    /// no extra call.
    /// </summary>
    /// <param name="filter">
    /// Paging, ordering and search parameters. The table itself does not page (it needs the whole
    /// tree in the DOM to reorder it), so an empty request is answered with one big page.
    /// </param>
    /// <param name="filters">
    /// Conditions built by the generic filter picker; ANDed together and resolved by the same
    /// shared engine the users list uses, so a filter means the same thing on every page.
    /// </param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// The flat <c>DataTableResult&lt;BlogCategoryListItemDto&gt;</c> envelope
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
            new GetBlogCategoriesForDataTableQuery(filter, filters), ct);

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
        var result = await dispatcher.Send(new GetBlogCategoryFilterFieldsQuery(), ct);

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
    /// <summary>Creates a blog category from the create modal (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateBlogCategoryCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.BlogCategories.Create)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an existing blog category from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the category id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateBlogCategoryCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.BlogCategories.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes a blog category from the delete modal (?handler=Delete).</summary>
    /// <param name="id">The identifier of the category to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] BlogCategoryId id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.BlogCategories.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteBlogCategoryCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="rows">The reordered rows in visual order; only the ids matter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromBody] List<ReorderBlogCategoryRow> rows, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.BlogCategories.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderBlogCategoriesCommand(rows.Select(r => r.Id).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves visual order).</summary>
    /// <param name="Id">The blog category identifier.</param>
    /// <param name="DisplayOrder">The visual order displayed by the badge.</param>
    public sealed record ReorderBlogCategoryRow(BlogCategoryId Id, int DisplayOrder);
}