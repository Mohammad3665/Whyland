using Whyland.Application.Features.Admin.BlogCategories.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.BlogPosts;
using Whyland.Application.Features.Admin.BlogPosts.Commands.Create;
using Whyland.Application.Features.Admin.BlogPosts.Commands.Delete;
using Whyland.Application.Features.Admin.BlogPosts.Commands.Reorder;
using Whyland.Application.Features.Admin.BlogPosts.Commands.Update;
using Whyland.Application.Features.Admin.BlogPosts.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.BlogPosts.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Users.Queries.GetOptions;
using Whyland.Domain.Model.Blog.BlogCategory.Identifiers;
using Whyland.Domain.Model.Blog.BlogPost.Entities;
using Whyland.Domain.Model.Blog.BlogPost.Identifiers;
using Whyland.Domain.Model.Blog.BlogPost.Repositories;
using Whyland.Shared.Dtos.Identity.User;
using Whyland.Shared.Enums;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Blog.BlogPosts;

/// <summary>
/// «مقالات» admin page (section 12.1.2 of the SRS): wired to the blog post feature commands and
/// queries through <see cref="IDispatcher"/>.
/// Rows are loaded through ?handler=Rows, the generic filter picker through ?handler=Fields and the
/// category/author selects through ?handler=FormOptions. Every mutation runs through the
/// corresponding command handler. The table is the same server-side DataTable engine the courses
/// page uses, so search, sorting and the picker conditions all resolve in SQL.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Blogs.View)]
public class BlogPostsModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService,
    ICurrentUserService currentUserService) : AdminPageModel(authorizationService)
{
    /// <summary>Renders the page scaffold; the table is filled through <see cref="OnGetRowsAsync"/>.</summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Returns one page of blog posts for the server-side DataTable, carrying every active picker
    /// condition. Each row includes the article body, the resolved author/category, both image URLs
    /// and the audit columns, so opening the edit or details modal costs no extra request.
    /// </summary>
    /// <param name="filter">Paging, sorting and free-text search parameters.</param>
    /// <param name="filters">Conditions built by the generic filter picker.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter, List<FilterCondition>? filters, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new GetBlogPostsForDataTableQuery(filter, filters), ct);

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
        var result = await dispatcher.Send(new GetBlogPostFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the two option lists the category form field and the category/author filter kinds
    /// need: the blog taxonomy in display order (with its level) and the accounts that may author
    /// a post. The author list is only used by the listing filter now; the form never changes who
    /// wrote an existing post.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFormOptionsAsync(CancellationToken ct)
    {
        // The taxonomy is uncapped and small; a single big page keeps the tree order the form shows.
        var categoryFilter = new DataTableFilter { Length = "10000", Start = "0" };

        var categoriesResult = await dispatcher.Send(
            new GetBlogCategoriesForDataTableQuery(categoryFilter), ct);
        if (categoriesResult.IsFailure)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(categoriesResult.Error) });

        var authorsResult = await dispatcher.Send(
            new GetUserOptionsQuery(nameof(BlogPost.AuthorId)), ct);
        if (authorsResult.IsFailure)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(authorsResult.Error) });

        var categories = (categoriesResult.Data?.Data ?? [])
            .Select(x => new { x.Id, x.Name, x.Depth, x.IsActive })
            .ToList();

        return new JsonResult(new
        {
            isSuccess = true,
            data = new { categories, authors = authorsResult.Data ?? new List<UserOptionDto>() },
        });
    }

    #region Mutations

    /// <summary>Creates a post from the full-screen form (?handler=Create).</summary>
    /// <param name="input">The multipart form fields, including the optional cover image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync([FromForm] BlogPostFormInput input, CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Blogs.Create);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            BlogPostImageUpload? image = null;

            if (input.Image is { Length: > 0 })
            {
                uploadStream = input.Image.OpenReadStream();
                image = new BlogPostImageUpload(
                    uploadStream,
                    input.Image.FileName,
                    input.Image.ContentType,
                    input.Image.Length);
            }

            // The signed-in operator is the author of a new post; the form does not carry an
            // author field. A request without a user id cannot be attributed and is rejected.
            var authorId = currentUserService.UserId;
            if (authorId is null)
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { isSuccess = false, message = "کاربر جاری شناسایی نشد." });

            var command = new CreateBlogPostCommand(
                input.Title,
                input.ShortDescription,
                input.Content,
                authorId.Value,
                input.CategoryId,
                input.Slug,
                input.DisplayOrder,
                input.DisplayStatus,
                image);

            return ToJson(await dispatcher.Send(command, ct));
        }
        finally
        {
            if (uploadStream is not null)
                await uploadStream.DisposeAsync();
        }
    }

    /// <summary>Updates a post from the full-screen form (?handler=Update).</summary>
    /// <param name="input">The multipart form fields, including the optional replacement image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync([FromForm] BlogPostFormInput input, CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Blogs.Edit);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            BlogPostImageUpload? image = null;

            if (input.Image is { Length: > 0 })
            {
                uploadStream = input.Image.OpenReadStream();
                image = new BlogPostImageUpload(
                    uploadStream,
                    input.Image.FileName,
                    input.Image.ContentType,
                    input.Image.Length);
            }

            var command = new UpdateBlogPostCommand(
                BlogPostId.From(input.Id),
                input.Title,
                input.ShortDescription,
                input.Content,
                input.CategoryId,
                input.Slug,
                input.DisplayOrder,
                input.DisplayStatus,
                input.RemoveImage,
                image);

            return ToJson(await dispatcher.Send(command, ct));
        }
        finally
        {
            if (uploadStream is not null)
                await uploadStream.DisposeAsync();
        }
    }

    /// <summary>Soft-deletes a post from the confirmation modal (?handler=Delete).</summary>
    /// <param name="id">The identifier of the post to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] BlogPostId id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Blogs.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteBlogPostCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="rows">The reordered rows in visual order, with the display-order slot each takes.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromBody] List<ReorderBlogPostRow> rows, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Blogs.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderBlogPostsCommand(
                    rows.Select(r => new BlogPostOrdering(BlogPostId.From(r.Id), r.DisplayOrder)).ToList()),
                ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves the visual order).</summary>
    /// <param name="Id">The blog post identifier.</param>
    /// <param name="DisplayOrder">The display-order slot the row now occupies.</param>
    public sealed record ReorderBlogPostRow(long Id, int DisplayOrder);

    /// <summary>
    /// The multipart shape the full-screen post form posts. It keeps ASP.NET's <see cref="IFormFile"/>
    /// lifetime out of the application command: the command carries a <see cref="BlogPostImageUpload"/>
    /// stream while the request is alive.
    /// </summary>
    public sealed class BlogPostFormInput
    {
        /// <summary>The post identifier; zero or absent when creating.</summary>
        public long Id { get; set; }

        /// <summary>The post title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>The excerpt shown in listings.</summary>
        public string ShortDescription { get; set; } = string.Empty;

        /// <summary>The full HTML body from the rich-text editor.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>The blog category the post is filed under.</summary>
        public BlogCategoryId CategoryId { get; set; }

        /// <summary>The unique URL-safe slug.</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>The ordering weight in the article listing.</summary>
        public int DisplayOrder { get; set; }

        /// <summary>The public visibility status.</summary>
        public DisplayStatus DisplayStatus { get; set; }

        /// <summary>Whether the existing cover image should be removed.</summary>
        public bool RemoveImage { get; set; }

        /// <summary>The uploaded replacement/cover image, when one was chosen.</summary>
        public IFormFile? Image { get; set; }
    }
}
