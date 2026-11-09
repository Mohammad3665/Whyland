using Whyland.Application.Features.Admin.Courses;
using Whyland.Application.Features.Admin.Courses.Commands.Create;
using Whyland.Application.Features.Admin.Courses.Commands.Delete;
using Whyland.Application.Features.Admin.Courses.Commands.Update;
using Whyland.Application.Features.Admin.Courses.Queries.GetById;
using Whyland.Application.Features.Admin.Courses.Queries.GetFilterFields;
using Whyland.Application.Features.Admin.Courses.Queries.GetForDataTable;
using Whyland.Application.Features.Admin.Courses.Queries.GetFormOptions;
using Whyland.Domain.Model.Catalog.Category.Identifiers;
using Whyland.Domain.Model.Catalog.Course.Enums;
using Whyland.Domain.Model.Catalog.Course.Identifiers;
using Whyland.Domain.Model.Identity.InstructorProfile.Identifiers;
using Whyland.Shared.Enums;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses;

/// <summary>
/// «دوره‌ها» admin page (section 7.2 of the SRS): wired to the course feature commands and queries
/// through <see cref="IDispatcher"/>.
/// <para>
/// Rows are loaded through <c>?handler=Rows</c> by the server-side DataTable, the generic filter
/// picker through <c>?handler=Fields</c>, the form and filter value lists through
/// <c>?handler=FormOptions</c> and the edit/details form through <c>?handler=Details</c>. The
/// category and instructor filters are entries of that same generic picker, so all filtering is
/// resolved by the one shared data-table engine.
/// </para>
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Courses.View)]
public class CoursesModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>Renders the page scaffold; the table body is filled through <see cref="OnGetRowsAsync"/>.</summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Returns one page of courses for the server-side DataTable. The table sends its paging,
    /// ordering and search parameters, and the generic picker sends its conditions; the query
    /// repository resolves them against its whitelist.
    /// </summary>
    /// <param name="filter">Paging, ordering and search parameters.</param>
    /// <param name="filters">
    /// Conditions built by the generic picker, including its category and instructor entries. A
    /// named filter field is matched to a whitelisted column; an unknown one is dropped by the
    /// engine.
    /// </param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetRowsAsync(
        DataTableFilter filter,
        List<FilterCondition>? filters,
        CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCoursesForDataTableQuery(filter, filters), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>Returns the fields the generic filter picker can build conditions for.</summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFieldsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseFilterFieldsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Returns the active categories and the instructors the form edits and the generic filter
    /// picker selects from.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetFormOptionsAsync(CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseFormOptionsQuery(), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>Returns the editable course with its selected category and instructor ids.</summary>
    /// <param name="id">The course identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetDetailsAsync(long id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseByIdQuery(CourseId.From(id)), ct);

        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, data = result.Data })
            : new JsonResult(new { isSuccess = false, message = MainMessage(result.Error) })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
    }

    #region Mutations

    /// <summary>Creates a course from the full-screen form (<c>?handler=Create</c>).</summary>
    /// <param name="input">The multipart form fields, including the optional cover image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnPostCreateAsync([FromForm] CourseFormInput input, CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Courses.Create);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            CourseImageUpload? image = null;

            if (input.Image is { Length: > 0 })
            {
                uploadStream = input.Image.OpenReadStream();
                image = new CourseImageUpload(
                    uploadStream,
                    input.Image.FileName,
                    input.Image.ContentType,
                    input.Image.Length);
            }

            var command = new CreateCourseCommand(
                input.Name,
                input.LatinName,
                input.ShortDescription,
                input.Description,
                input.Slug,
                input.Price,
                input.DiscountType,
                input.DiscountValue,
                input.IsSaleActive,
                input.DisplayStatus,
                input.IsFeatured,
                input.IsAmazing,
                input.CategoryIds ?? [],
                input.InstructorIds ?? [],
                image);

            return ToJson(await dispatcher.Send(command, ct));
        }
        finally
        {
            if (uploadStream is not null)
                await uploadStream.DisposeAsync();
        }
    }

    /// <summary>Updates a course from the full-screen form (<c>?handler=Update</c>).</summary>
    /// <param name="input">The multipart form fields, including the optional replacement image.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnPostUpdateAsync([FromForm] CourseFormInput input, CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Courses.Edit);
        if (denied is not null)
            return denied;

        Stream? uploadStream = null;

        try
        {
            CourseImageUpload? image = null;

            if (input.Image is { Length: > 0 })
            {
                uploadStream = input.Image.OpenReadStream();
                image = new CourseImageUpload(
                    uploadStream,
                    input.Image.FileName,
                    input.Image.ContentType,
                    input.Image.Length);
            }

            var courseId = CourseId.From(input.Id);

            var command = new UpdateCourseCommand(
                courseId,
                input.Name,
                input.LatinName,
                input.ShortDescription,
                input.Description,
                input.Slug,
                input.Price,
                input.DiscountType,
                input.DiscountValue,
                input.IsSaleActive,
                input.DisplayStatus,
                input.IsFeatured,
                input.IsAmazing,
                input.CategoryIds ?? [],
                input.InstructorIds ?? [],
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

    /// <summary>Soft-deletes a course from the confirmation modal (<c>?handler=Delete</c>).</summary>
    /// <param name="id">The identifier of the course to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Delete)
            ?? ToJson(await dispatcher.Send(new DeleteCourseCommand(CourseId.From(id)), ct));

    #endregion

    /// <summary>
    /// The multipart shape the full-screen course form posts. It keeps the page out of the
    /// application command: the command carries an <see cref="CourseImageUpload"/> stream, while
    /// this model carries the ASP.NET <see cref="IFormFile"/> whose lifetime ends with the request.
    /// </summary>
    public sealed class CourseFormInput
    {
        /// <summary>The course identifier; zero or absent when creating.</summary>
        public long Id { get; set; }

        /// <summary>The Persian/course display name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>The Latin name used to build the slug.</summary>
        public string LatinName { get; set; } = string.Empty;

        /// <summary>The excerpt shown in listings.</summary>
        public string ShortDescription { get; set; } = string.Empty;

        /// <summary>The full HTML description from the rich-text editor.</summary>
        public string? Description { get; set; }

        /// <summary>The unique URL-safe slug.</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>The price in Toman; null means free.</summary>
        public decimal? Price { get; set; }

        /// <summary>The discount kind (None, Percentage, FixedAmount).</summary>
        public DiscountType DiscountType { get; set; } = DiscountType.None;

        /// <summary>The discount amount, when one is set.</summary>
        public decimal? DiscountValue { get; set; }

        /// <summary>Whether the course can currently be purchased.</summary>
        public bool IsSaleActive { get; set; } = true;

        /// <summary>The visibility status (Draft, Published, Hidden).</summary>
        public DisplayStatus DisplayStatus { get; set; } = DisplayStatus.Draft;

        /// <summary>Whether the course is promoted as featured.</summary>
        public bool IsFeatured { get; set; }

        /// <summary>Whether the course appears in the promotional deals section.</summary>
        public bool IsAmazing { get; set; }

        /// <summary>The selected category identifiers.</summary>
        public List<CategoryId>? CategoryIds { get; set; } = [];

        /// <summary>The selected instructor profile identifiers.</summary>
        public List<InstructorProfileId>? InstructorIds { get; set; } = [];

        /// <summary>Whether the existing cover image should be removed.</summary>
        public bool RemoveImage { get; set; }

        /// <summary>The uploaded replacement/cover image, when one was chosen.</summary>
        public IFormFile? Image { get; set; }
    }
}
