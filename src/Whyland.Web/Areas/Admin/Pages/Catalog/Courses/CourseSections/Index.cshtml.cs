using Whyland.Application.Features.Admin.CourseSections.Commands.Create;
using Whyland.Application.Features.Admin.CourseSections.Commands.Delete;
using Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;
using Whyland.Application.Features.Admin.CourseSections.Commands.Update;
using Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;
using Whyland.Application.Features.Admin.Courses.Queries.GetById;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.CourseSections;

/// <summary>
/// «سرفصل‌های دوره» page: sections of one course, ordered by DisplayOrder and managed only from
/// the course page. Rows and every mutation go through the course-section feature handlers.
/// </summary>
[HasPermission(Permissions.Courses.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService) : AdminPageModel(authorizationService)
{
    /// <summary>The course whose sections are being managed.</summary>
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

    /// <summary>Returns all live sections of the course, ordered by DisplayOrder.</summary>
    public async Task<IActionResult> OnGetRowsAsync([FromQuery] long courseId, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetCourseSectionsQuery(courseId), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    /// <summary>Creates a section in the course (?handler=Create).</summary>
    public async Task<IActionResult> OnPostCreateAsync(CreateCourseSectionCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Updates a section (?handler=Update).</summary>
    public async Task<IActionResult> OnPostUpdateAsync(UpdateCourseSectionCommand command, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(command, ct));

    /// <summary>Soft-deletes an empty section (?handler=Delete).</summary>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(new DeleteCourseSectionCommand(id), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromQuery] long courseId,
        [FromBody] List<ReorderRow> rows,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderCourseSectionsCommand(courseId, rows.Select(r => r.Id).ToList()), ct));

    #endregion

    /// <summary>Reorder payload sent by the client.</summary>
    public sealed record ReorderRow(long Id, int DisplayOrder);
}
