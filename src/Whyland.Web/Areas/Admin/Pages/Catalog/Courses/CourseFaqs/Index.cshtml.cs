using Whyland.Application.Features.Admin.CourseFaqs.Commands.Create;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Delete;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;
using Whyland.Application.Features.Admin.CourseFaqs.Commands.Update;
using Whyland.Application.Features.Admin.CourseFaqs.Queries.GetForCourse;
using Whyland.Application.Features.Admin.Courses.Queries.GetById;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.CourseFaqs;

/// <summary>
/// «سوالات متداول دوره» page: the questions attached to one course, managed only from the course
/// page. The handlers mirror the site FAQ page, but every row is scoped to a course id.
/// </summary>
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

    /// <summary>Loads the page and resolves the course title; an unknown course is a 404.</summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (CourseId <= 0)
            return NotFound();

        var course = await dispatcher.Send(new GetCourseByIdQuery(CourseId), ct);
        if (!course.IsSuccess)
            return NotFound();

        CourseTitle = course.Data!.Name;
        return Page();
    }

    /// <summary>Returns all live FAQs of the course, ordered by DisplayOrder.</summary>
    public async Task<IActionResult> OnGetRowsAsync([FromQuery] long courseId, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseFaqsQuery(courseId), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    /// <summary>Creates a FAQ in the course (?handler=Create).</summary>
    public async Task<IActionResult> OnPostCreateAsync(CreateCourseFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates a FAQ (?handler=Update).</summary>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateCourseFaqCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes a FAQ (?handler=Delete).</summary>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(new DeleteCourseFaqCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromQuery] long courseId,
        [FromBody] List<ReorderRow> rows,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderCourseFaqsCommand(courseId, rows.Select(r => r.Id).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client.</summary>
    public sealed record ReorderRow(long Id, int DisplayOrder);
}
