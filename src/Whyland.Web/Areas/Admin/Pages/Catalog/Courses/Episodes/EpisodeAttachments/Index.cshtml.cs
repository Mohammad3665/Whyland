using Microsoft.Extensions.Options;

using Whyland.Application.Common.Options;
using Whyland.Application.Features.Admin.CourseSections.Queries.GetForCourse;
using Whyland.Application.Features.Admin.Courses.Queries.GetById;
using Whyland.Application.Features.Admin.EpisodeAttachments;
using Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Delete;
using Whyland.Application.Features.Admin.EpisodeAttachments.Commands.Upload;
using Whyland.Application.Features.Admin.EpisodeAttachments.Queries.GetForEpisode;
using Whyland.Application.Features.Admin.Episodes.Queries.GetHeader;
using Whyland.Domain.Model.Catalog.Course.Identifiers;
using Whyland.Domain.Model.Catalog.CourseSection.Identifiers;
using Whyland.Domain.Model.Catalog.Episode.Identifiers;
using Whyland.Domain.Model.Catalog.EpisodeAttachment.Identifiers;

namespace Whyland.Web.Areas.Admin.Pages.Catalog.Courses.Episodes.EpisodeAttachments;

/// <summary>
/// «پیوست‌های اپیزود» page: the downloadable files of one episode, with the upload limits the server
/// enforces (max count, max size, allowed extensions). Rows are loaded through ?handler=Rows; uploads
/// and deletes run through their command handlers. See wwwroot/admin/js/episode-attachments.js.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch commands and queries.</param>
/// <param name="authorizationService">Checks the write key of each handler, which a page attribute cannot express.</param>
/// <param name="options">The configured attachment limits (s7.8).</param>
[HasPermission(Permissions.Courses.View)]
public class IndexModel(
    IDispatcher dispatcher,
    IAuthorizationService authorizationService,
    IOptions<EpisodeAttachmentOptions> options) : AdminPageModel(authorizationService)
{
    private readonly EpisodeAttachmentOptions _options = options.Value;

    /// <summary>The course the episode ultimately belongs to; used for the back links.</summary>
    [BindProperty(SupportsGet = true)]
    public long CourseId { get; set; }

    /// <summary>The section of the episode; used for the back link.</summary>
    [BindProperty(SupportsGet = true)]
    public long SectionId { get; set; }

    /// <summary>The episode whose attachments are being managed.</summary>
    [BindProperty(SupportsGet = true)]
    public long EpisodeId { get; set; }

    /// <summary>The course title shown in the page header.</summary>
    public string CourseTitle { get; private set; } = string.Empty;

    /// <summary>The section title shown in the page header.</summary>
    public string SectionTitle { get; private set; } = string.Empty;

    /// <summary>The episode title shown in the page header.</summary>
    public string EpisodeTitle { get; private set; } = string.Empty;

    /// <summary>The configured maximum number of attachments per episode (s7.8).</summary>
    public int MaxAttachmentsPerEpisode => _options.MaxAttachmentsPerEpisode;

    /// <summary>The configured maximum attachment size, in bytes (s7.8).</summary>
    public long MaxAttachmentSizeBytes => _options.MaxAttachmentSizeBytes;

    /// <summary>The configured accepted extensions, each with its leading dot.</summary>
    public IReadOnlyList<string> AllowedAttachmentExtensions => _options.AllowedExtensions;

    /// <summary>Human-readable max size for the modal hint (e.g. "100 مگابایت").</summary>
    public string MaxAttachmentSizeDisplay =>
        $"{Math.Max(1, MaxAttachmentSizeBytes / (1024 * 1024))} مگابایت";

    /// <summary>Comma-separated allowed extensions for the modal hint.</summary>
    public string AllowedExtensionsDisplay => string.Join("، ", AllowedAttachmentExtensions);

    /// <summary>
    /// Loads the page and resolves the course, section and episode titles; an unknown episode, a
    /// mismatched section/course, or an unknown parent is a 404.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (CourseId <= 0 || SectionId <= 0 || EpisodeId <= 0)
            return NotFound();

        var episode = await dispatcher.Send(new GetEpisodeHeaderQuery(Whyland.Domain.Model.Catalog.Episode.Identifiers.EpisodeId.From(EpisodeId)), ct);

        if (!episode.IsSuccess
            || episode.Data!.SectionId != SectionId
            || episode.Data.CourseId != CourseId)
        {
            return NotFound();
        }

        var sections = await dispatcher.Send(
            new GetCourseSectionsQuery(Whyland.Domain.Model.Catalog.Course.Identifiers.CourseId.From(CourseId)),
            ct);

        var section = sections.IsSuccess
            ? sections.Data!.FirstOrDefault(s => s.Id == SectionId)
            : null;

        var course = await dispatcher.Send(new GetCourseByIdQuery(Whyland.Domain.Model.Catalog.Course.Identifiers.CourseId.From(CourseId)), ct);
        if (section is null || !course.IsSuccess)
            return NotFound();

        CourseTitle = course.Data!.Name;
        SectionTitle = section.Title;
        EpisodeTitle = episode.Data.Title;
        return Page();
    }

    /// <summary>
    /// Returns the live attachments of the episode, in upload order, that the client-side DataTable
    /// consumes.
    /// </summary>
    /// <param name="episodeId">The episode whose attachments are read.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The attachment rows as JSON, or a 500 status with an error message.</returns>
    public async Task<IActionResult> OnGetRowsAsync([FromQuery] long episodeId, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new GetEpisodeAttachmentsQuery(Whyland.Domain.Model.Catalog.Episode.Identifiers.EpisodeId.From(episodeId)),
            ct);

        return result.IsSuccess
            ? new JsonResult(result.Data)
            : StatusCode(
                StatusCodes.Status500InternalServerError,
                new { isSuccess = false, message = MainMessage(result.Error) });
    }

    #region Mutations

    /// <summary>
    /// Stores one uploaded attachment (?handler=Upload). The multipart shape is read here; the
    /// application command only sees the stream.
    /// </summary>
    /// <param name="episodeId">The episode that receives the file.</param>
    /// <param name="file">The uploaded file.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostUploadAsync(
        [FromForm] long episodeId,
        IFormFile? file,
        CancellationToken ct)
    {
        var denied = await DenyUnlessAsync(Permissions.Courses.Edit);
        if (denied is not null)
            return denied;

        if (file is null)
        {
            return new JsonResult(new { isSuccess = false, message = "فایل پیوست انتخاب نشده است." })
            {
                StatusCode = StatusCodes.Status400BadRequest,
            };
        }

        await using var stream = file.OpenReadStream();

        var upload = new EpisodeAttachmentUpload(stream, file.FileName, file.ContentType, file.Length);

        return ToJson(await dispatcher.Send(
            new UploadEpisodeAttachmentCommand(Whyland.Domain.Model.Catalog.Episode.Identifiers.EpisodeId.From(episodeId), upload),
            ct));
    }

    /// <summary>Soft-deletes an attachment and removes its protected file (?handler=Delete).</summary>
    /// <param name="id">The identifier of the attachment to delete.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A flat <c>{isSuccess, message}</c> envelope.</returns>
    public async Task<IActionResult> OnPostDeleteAsync([FromForm] long id, CancellationToken ct)
        => await DenyUnlessAsync(Permissions.Courses.Edit)
            ?? ToJson(await dispatcher.Send(
                new DeleteEpisodeAttachmentCommand(EpisodeAttachmentId.From(id)),
                ct));

    #endregion
}
