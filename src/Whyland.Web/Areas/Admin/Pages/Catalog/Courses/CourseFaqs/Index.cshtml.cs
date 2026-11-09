using Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;
using Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;
using Whyland.Application.Features.Admin.Courses.Queries.GetById;
using Whyland.Domain.Model.Catalog.Course.Identifiers;
using Whyland.Domain.Model.Catalog.CourseFaq.Identifiers;
using CourseIdentifier = Whyland.Domain.Model.Catalog.Course.Identifiers.CourseId;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.CourseFaqs;

/// <summary>
/// «سوالات متداول دوره» page: the questions attached to one course, managed only from the course
/// page. Rows are loaded through ?handler=Rows and every mutation runs through the corresponding
/// command handler; the table itself is a client-side DataTable with RowReorder drag &amp; drop
/// ordering (see wwwroot/admin/js/course-faqs.js).
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
[HasPermission(Permissions.Courses.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>The course whose FAQs are being managed.</summary>
    [BindProperty(SupportsGet = true)]
    public long CourseId { get; set; }

    /// <summary>The course title shown in the page header.</summary>
    public string CourseTitle { get; private set; } = string.Empty;

    /// <summary>
    /// Loads the page and resolves the course title; an unknown course is a 404.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (CourseId <= 0)
            return NotFound();

        var courseId = CourseIdentifier.From(CourseId);
        var course = await dispatcher.Send(new GetCourseByIdQuery(courseId), ct);
        if (!course.IsSuccess)
            return NotFound();

        CourseTitle = course.Data!.Name;
        return Page();
    }

    /// <summary>
    /// Returns all live FAQ items of the course, ordered by DisplayOrder, that the client-side
    /// DataTable and its drag &amp; drop ordering consume.
    /// </summary>
    /// <param name="courseId">The course whose questions are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// The question rows as JSON, or a 500 status with an error message.
    /// </returns>
    public async Task<IActionResult> OnGetRowsAsync([FromQuery] long courseId, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseFaqsQuery(CourseIdentifier.From(courseId)), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    // Each write demands the course edit key on top of the view key the class carries: opening the
    // list must not be enough to change it. The check is made in code because an attribute on a page
    // handler method is ignored by Razor Pages (MVC1001).
    /// <summary>Creates a FAQ in the course (?handler=Create).</summary>
    /// <param name="command">The values submitted in the modal form.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(CreateCourseFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates an existing question from the edit modal (?handler=Update).</summary>
    /// <param name="command">The values submitted in the modal form, including the question id.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateCourseFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes a question from the delete modal (?handler=Delete).</summary>
    /// <param name="id">The identifier of the question to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] CourseFaqId id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(new DeleteCourseFaqCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="courseId">The course whose questions are reordered.</param>
    /// <param name="rows">The reordered rows in visual order; only the ids matter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromQuery] long courseId,
        [FromBody] List<ReorderRow> rows,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderCourseFaqsCommand(CourseIdentifier.From(courseId), rows.Select(r => CourseFaqId.From(r.Id)).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves visual order).</summary>
    /// <param name="Id">The question identifier.</param>
    /// <param name="DisplayOrder">The visual order displayed by the badge.</param>
    public sealed record ReorderRow(long Id, int DisplayOrder);
}
