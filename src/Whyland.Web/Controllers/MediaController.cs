using Whyland.Application.Features.Media.EpisodeAttachments.Queries.GetAttachmentDownload;
using Whyland.Application.Features.Media.Episodes.Queries.GetEpisodeVideo;
using Whyland.Domain.Model.Catalog.Episode.Identifiers;
using Whyland.Domain.Model.Catalog.EpisodeAttachment.Identifiers;
using Whyland.Shared.Errors;

namespace Whyland.Web.Controllers;

/// <summary>
/// The only public way to reach a protected media file (s8.1, s8.2). Static serving of the media
/// root is blocked, so every playback and download passes through these actions: access is decided
/// by the application layer before the storage adapter opens the file, and the response streams it
/// with range support for video seeking.
/// </summary>
/// <param name="dispatcher">The interface used to dispatch the media queries.</param>
/// <param name="currentUserService">Resolves the signed-in account, or null for an anonymous caller.</param>
/// <param name="authorizationService">Checks the course-management permission that grants staff preview.</param>
public sealed class MediaController(
    IDispatcher dispatcher,
    ICurrentUserService currentUserService,
    IAuthorizationService authorizationService) : Controller
{
    /// <summary>
    /// Streams the protected MP4 of an episode (s8.1.3). The response is private and never cached;
    /// range requests are handled by the file result so the player can seek.
    /// </summary>
    /// <param name="id">The episode identifier from the route.</param>
    /// <param name="ct">A token to cancel the request.</param>
    /// <returns>The video stream, or a 403/404/500 when access or lookup fails.</returns>
    [HttpGet("media/episodes/{id:long}/video")]
    public async Task<IActionResult> EpisodeVideo(long id, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new GetEpisodeVideoQuery(
                EpisodeId.From(id),
                currentUserService.UserId,
                await IsStaffAsync()),
            ct);

        if (result.IsFailure)
            return MediaError(result.Error);

        var source = result.Data!;

        // An external video is authorized exactly like a stored one; the browser is then sent to the
        // host. The URL is re-validated here so a hand-edited row can never redirect anywhere else.
        if (source.IsExternal)
        {
            if (!Uri.TryCreate(source.ExternalUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return NotFound();
            }

            SetProtectedHeaders();
            return Redirect(source.ExternalUrl!);
        }

        if (source.File is null)
            return NotFound();

        return MediaFile(source.File.Content, "video/mp4");
    }

    /// <summary>
    /// Streams one protected attachment with its stored download name (s8.2.1). The response is
    /// private and never cached.
    /// </summary>
    /// <param name="id">The attachment identifier from the route.</param>
    /// <param name="ct">A token to cancel the request.</param>
    /// <returns>The attachment stream, or a 403/404/500 when access or lookup fails.</returns>
    [HttpGet("media/attachments/{id:long}")]
    public async Task<IActionResult> Attachment(long id, CancellationToken ct)
    {
        var result = await dispatcher.Send(
            new GetEpisodeAttachmentDownloadQuery(
                EpisodeAttachmentId.From(id),
                currentUserService.UserId,
                await IsStaffAsync()),
            ct);

        if (result.IsFailure)
            return MediaError(result.Error);

        var file = result.Data!;

        SetProtectedHeaders();
        return File(
            file.Content,
            file.ContentType,
            file.FileName,
            enableRangeProcessing: true);
    }

    /// <summary>
    /// Whether the caller may preview protected material as staff. The page gate is not enough:
    /// this endpoint is outside the admin area, so the permission is checked here again.
    /// </summary>
    private async Task<bool> IsStaffAsync()
        => (await authorizationService.AuthorizeAsync(User, null, Permissions.Courses.View)).Succeeded;

    /// <summary>Builds the streaming response of a video file with the headers s8.1.4 requires.</summary>
    private FileStreamResult MediaFile(Stream content, string contentType)
    {
        SetProtectedHeaders();
        return File(content, contentType, enableRangeProcessing: true);
    }

    /// <summary>
    /// Marks the response as private and uncacheable and forbids content sniffing, so a shared cache
    /// never stores protected material and a browser never reinterprets it (s8.1.4).
    /// </summary>
    private void SetProtectedHeaders()
    {
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        // A known URL must not become an embeddable asset on another origin. The player, admin
        // preview and download all run on this origin, so same-origin is the narrowest policy.
        Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
    }

    /// <summary>
    /// Maps an application failure to the matching status code. Details stay in the logs; the body
    /// carries no path or storage information (s8.6).
    /// </summary>
    /// <param name="error">The failure returned by the media query.</param>
    private IActionResult MediaError(Error error) => error.Type switch
    {
        ErrorType.NotFound => NotFound(),
        ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden),
        ErrorType.Unauthorized => StatusCode(StatusCodes.Status401Unauthorized),
        _ => StatusCode(StatusCodes.Status500InternalServerError),
    };
}
