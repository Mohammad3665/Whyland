using Microsoft.Extensions.Options;

using Whyland.Application.Features.Admin.Courses.Queries.GetById;
using Whyland.Application.Features.Admin.EpisodeAttachments.Queries.GetForEpisode;
using Whyland.Application.Features.Admin.Episodes.Commands.Create;
using Whyland.Application.Features.Admin.Episodes.Commands.Delete;
using Whyland.Application.Features.Admin.Episodes.Commands.RemoveVideo;
using Whyland.Application.Features.Admin.Episodes.Commands.Reorder;
using Whyland.Application.Features.Admin.Episodes.Commands.SetVideoLink;
using Whyland.Application.Features.Admin.Episodes.Commands.Update;
using Whyland.Application.Features.Admin.Episodes.Commands.UploadVideo;
using Whyland.Application.Features.Admin.Episodes.Queries.GetForSection;
using Whyland.Application.Features.Admin.Episodes.Queries.GetSectionInfo;
using Whyland.Application.Features.Admin.Episodes;
using Whyland.Application.Common.Helpers;
using Whyland.Domain.Model.Catalog.Course.Identifiers;
using Whyland.Domain.Model.Catalog.CourseSection.Identifiers;
using Whyland.Domain.Model.Catalog.Episode.Identifiers;
using Whyland.Shared.Filters;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.Episodes;

/// <summary>
/// «اپیزودها» page: the episodes of one course section, ordered by DisplayOrder and reached from
/// the sections page. Rows are loaded through ?handler=Rows; offsets, deletes, drag &amp; drop
/// ordering and the protected video upload all run through their command handlers. The table is a
/// client-side DataTable with RowReorder (see wwwroot/admin/js/episodes.js).
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
/// <param name="videoOptions">The configured video ceiling shown in the upload modal (s8.3.2).</param>
[HasPermission(Permissions.Courses.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService,
    IOptions<Whyland.Application.Common.Options.EpisodeVideoOptions> videoOptions) : AdminPageModel(authorizationService)
{
    private readonly Whyland.Application.Common.Options.EpisodeVideoOptions _videoOptions = videoOptions.Value;

    /// <summary>The course the section belongs to; used for the back link and the header.</summary>
    [BindProperty(SupportsGet = true)]
    public long CourseId { get; set; }

    /// <summary>The section whose episodes are being managed.</summary>
    [BindProperty(SupportsGet = true)]
    public long SectionId { get; set; }

    /// <summary>The course title shown in the page header.</summary>
    public string CourseTitle { get; private set; } = string.Empty;

    /// <summary>The section title shown in the page header.</summary>
    public string SectionTitle { get; private set; } = string.Empty;

    /// <summary>The largest accepted video file, in bytes, rendered as a modal hint.</summary>
    public long MaxVideoSizeBytes => _videoOptions.MaxFileSizeBytes;

    /// <summary>Human-readable max video size for the modal hint (e.g. "500 مگابایت").</summary>
    public string MaxVideoSizeDisplay =>
        $"{Math.Max(1, MaxVideoSizeBytes / (1024 * 1024))} مگابایت";

    /// <summary>
    /// Loads the page and resolves the course and section titles; an unknown section, a section of a
    /// different course, or an unknown course is a 404.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (CourseId <= 0 || SectionId <= 0)
            return NotFound();

        var section = await dispatcher.Send(
            new GetEpisodeSectionInfoQuery(CourseSectionId.From(SectionId)),
            ct);

        if (!section.IsSuccess || section.Data!.CourseId != CourseId)
            return NotFound();

        var course = await dispatcher.Send(new GetCourseByIdQuery(Whyland.Domain.Model.Catalog.Course.Identifiers.CourseId.From(CourseId)), ct);
        if (!course.IsSuccess)
            return NotFound();

        CourseTitle = course.Data!.Name;
        SectionTitle = section.Data.Title;
        return Page();
    }

    /// <summary>
    /// Returns the live attachments of one episode for the details modal, which renders them as
    /// protected download links.
    /// </summary>
    /// <param name="id">The episode whose attachments are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The attachment rows as JSON, or a 500 status with an error message.</returns>
    public async Task<IActionResult> OnGetAttachmentsAsync([FromQuery] long id, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetEpisodeAttachmentsQuery(EpisodeId.From(id)), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    /// <summary>
    /// Describes the episode properties the generic filter picker can filter on. The table is
    /// client-side, so these descriptors drive filtering over the rows already loaded.
    /// </summary>
    public IActionResult OnGetFields()
        => new JsonResult(new
        {
            isSuccess = true,
            data = new[]
            {
                new FilterFieldDto("id", "شناسه", "number", FilterFieldCatalog.Range),
                new FilterFieldDto("title", "عنوان", "string", FilterFieldCatalog.Text),
                new FilterFieldDto("displayOrder", "ترتیب نمایش", "number", FilterFieldCatalog.Range),
                new FilterFieldDto("durationSeconds", "مدت زمان (ثانیه)", "number", FilterFieldCatalog.Range),
                new FilterFieldDto("isFree", "رایگان", "boolean", FilterFieldCatalog.Equality),
                new FilterFieldDto("hasVideo", "دارای ویدیو", "boolean", FilterFieldCatalog.Equality),
                new FilterFieldDto("attachmentCount", "تعداد پیوست", "number", FilterFieldCatalog.Range),
                new FilterFieldDto("createdAt", "تاریخ ایجاد", "date", FilterFieldCatalog.Range),
            },
        });

    /// <summary>
    /// Returns all live episodes of the section, ordered by DisplayOrder, that the client-side
    /// DataTable and its drag &amp; drop ordering consume.
    /// </summary>
    /// <param name="sectionId">The section whose episodes are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The episode rows as JSON, or a 500 status with an error message.</returns>
    public async Task<IActionResult> OnGetRowsAsync([FromQuery] long sectionId, CancellationToken ct)
    {
        var result = await dispatcher.Send(new GetEpisodesQuery(CourseSectionId.From(sectionId)), ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    // Each write demands the course edit key on top of the view key the class carries: opening the
    // list must not be enough to change it.

    /// <summary>Creates an episode in the section (?handler=Create).</summary>
    /// <param name="title">The episode title.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="displayOrder">The requested ordering position.</param>
    /// <param name="isFree">Whether the episode is a free preview.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostCreateAsync(
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] int displayOrder,
        [FromForm] bool isFree,
        CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Courses.Edit);
        if (denied is not null)
            return denied;

        var result = await dispatcher.Send(
            new CreateEpisodeCommand(
                CourseSectionId.From(SectionId),
                title,
                description,
                displayOrder,
                isFree),
            ct);

        // The client needs the new id to attach a video file or link right after the metadata save.
        return result.IsSuccess
            ? new JsonResult(new { isSuccess = true, message = (string?)null, id = result.Data!.Value })
            : ToJson(result);
    }

    /// <summary>Updates an existing episode from the edit modal (?handler=Update).</summary>
    /// <param name="id">The episode identifier.</param>
    /// <param name="title">The episode title.</param>
    /// <param name="description">The optional description.</param>
    /// <param name="displayOrder">The requested ordering position.</param>
    /// <param name="isFree">Whether the episode is a free preview.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUpdateAsync(
        [FromForm] long id,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] int displayOrder,
        [FromForm] bool isFree,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new UpdateEpisodeCommand(
                    EpisodeId.From(id),
                    title,
                    description,
                    displayOrder,
                    isFree),
                ct));

    /// <summary>Soft-deletes an episode that no longer has attachments (?handler=Delete).</summary>
    /// <param name="id">The identifier of the episode to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(new DeleteEpisodeCommand(EpisodeId.From(id)), ct));

    /// <summary>Persists the visual drag &amp; drop ordering (?handler=Reorder).</summary>
    /// <param name="sectionId">The section whose episodes are reordered (from the query string).</param>
    /// <param name="rows">The reordered rows in visual order; only the ids matter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostReorderAsync(
        [FromQuery] long sectionId,
        [FromBody] List<ReorderRow> rows,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new ReorderEpisodesCommand(
                    CourseSectionId.From(sectionId),
                    rows.Select(r => EpisodeId.From(r.Id)).ToList()),
                ct));

    /// <summary>
    /// Stores the uploaded MP4 as the episode's protected video (?handler=UploadVideo). The
    /// multipart shape is read here; the application command only sees the stream.
    /// </summary>
    /// <param name="id">The episode identifier.</param>
    /// <param name="file">The uploaded MP4 file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUploadVideoAsync(
        [FromForm] long id,
        IFormFile? file,
        CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Courses.Edit);
        if (denied is not null)
            return denied;

        if (file is null)
        {
            return new JsonResult(new { isSuccess = false, message = "فایل ویدیو انتخاب نشده است." })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }

        await using var stream = file.OpenReadStream();

        var upload = new EpisodeVideoUpload(stream, file.FileName, file.ContentType, file.Length);

        return ToJson(await dispatcher.Send(
            new UploadEpisodeVideoCommand(EpisodeId.From(id), upload),
            ct));
    }

    /// <summary>
    /// Points the episode at an externally hosted video instead of an uploaded file
    /// (?handler=SetVideoLink).
    /// </summary>
    /// <param name="id">The episode identifier.</param>
    /// <param name="url">The absolute http(s) link to the video.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostSetVideoLinkAsync(
        [FromForm] long id,
        [FromForm] string? url,
        CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new SetEpisodeVideoLinkCommand(EpisodeId.From(id), url ?? string.Empty),
                ct));

    /// <summary>Detaches and deletes the protected video of an episode (?handler=RemoveVideo).</summary>
    /// <param name="id">The episode identifier.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostRemoveVideoAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(new RemoveEpisodeVideoCommand(EpisodeId.From(id)), ct));

    #endregion

    /// <summary>Reorder payload sent by the client (RowReorder saves visual order).</summary>
    /// <param name="Id">The episode identifier.</param>
    /// <param name="DisplayOrder">The visual order displayed by the badge.</param>
    public sealed record ReorderRow(long Id, int DisplayOrder);
}
