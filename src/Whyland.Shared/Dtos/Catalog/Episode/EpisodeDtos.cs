using Whyland.Shared.Enums;

namespace Whyland.Shared.Dtos.Catalog.Episode;

/// <summary>
/// Read model representing one episode of a course section in the admin list.
/// </summary>
/// <param name="Id">The unique identifier of the episode.</param>
/// <param name="SectionId">The section the episode belongs to.</param>
/// <param name="Title">The episode title.</param>
/// <param name="Description">The optional description shown in the edit modal.</param>
/// <param name="DisplayOrder">Manual ordering weight; lower values appear first.</param>
/// <param name="IsFree">Whether the episode is a free preview (s7.7.3).</param>
/// <param name="DurationSeconds">The video duration in whole seconds; null when the episode has no video.</param>
/// <param name="HasVideo">Whether a video (protected file or external link) is attached.</param>
/// <param name="AttachmentCount">How many live attachments the episode currently has.</param>
/// <param name="CreatedAt">The UTC timestamp when the episode was created.</param>
/// <param name="VideoUrl">
/// The external video link when the episode uses one; null for a protected file or no video. The
/// storage key itself is never sent to the admin list.
/// </param>
public sealed record EpisodeListItemDto(
    long Id,
    long SectionId,
    string Title,
    string Description,
    int DisplayOrder,
    bool IsFree,
    DateTime CreatedAt,
    int DurationSeconds,
    bool HasVideo,
    int AttachmentCount,
    string? VideoUrl);

/// <summary>
/// Minimal section data the episodes page needs for its header and its back link.
/// </summary>
/// <param name="Id">The section identifier.</param>
/// <param name="CourseId">The course the section belongs to.</param>
/// <param name="Title">The section title.</param>
public sealed record EpisodeSectionInfoDto(long Id, long CourseId, string Title);

/// <summary>
/// Minimal episode data the attachment page needs for its header and its two back links.
/// </summary>
/// <param name="Id">The episode identifier.</param>
/// <param name="SectionId">The section the episode belongs to.</param>
/// <param name="CourseId">The course the episode ultimately belongs to.</param>
/// <param name="Title">The episode title.</param>
public sealed record EpisodeHeaderInfoDto(long Id, long SectionId, long CourseId, string Title);

/// <summary>
/// Everything the playback endpoint must know about an episode before deciding whether the caller may
/// watch it: where the file lives, how long it is, and the access inputs of s7.7.3 and s8.1.3.
/// </summary>
/// <param name="Id">The episode identifier.</param>
/// <param name="CourseId">The course the episode ultimately belongs to.</param>
/// <param name="Video">The server-generated storage key of the MP4 file, or null when none is attached.</param>
/// <param name="DurationSeconds">The stored duration in whole seconds.</param>
/// <param name="IsFree">Whether the episode is a free preview.</param>
/// <param name="CourseDisplayStatus">The public display status of the owning course.</param>
public sealed record EpisodePlaybackInfoDto(
    long Id,
    long CourseId,
    string? Video,
    int? DurationSeconds,
    bool IsFree,
    DisplayStatus CourseDisplayStatus);

/// <summary>
/// Read model for one downloadable attachment of an episode, used by the admin list.
/// </summary>
/// <param name="Id">The attachment identifier.</param>
/// <param name="EpisodeId">The episode the attachment belongs to.</param>
/// <param name="FileName">The display/download file name.</param>
/// <param name="ContentType">The MIME type recorded for the file.</param>
/// <param name="Size">The file size in bytes.</param>
/// <param name="CreatedAt">The UTC timestamp when the file was uploaded.</param>
public sealed record EpisodeAttachmentListItemDto(
    long Id,
    long EpisodeId,
    string FileName,
    string ContentType,
    long Size,
    DateTime CreatedAt);

/// <summary>
/// The metadata the download endpoint needs after access has been granted.
/// </summary>
/// <param name="Id">The attachment identifier.</param>
/// <param name="EpisodeId">The episode the attachment belongs to.</param>
/// <param name="FileName">The display/download file name.</param>
/// <param name="ContentType">The MIME type recorded for the file.</param>
/// <param name="StoragePath">The server-generated storage key under protected storage.</param>
/// <param name="Size">The file size in bytes.</param>
public sealed record EpisodeAttachmentDownloadDto(
    long Id,
    long EpisodeId,
    string FileName,
    string ContentType,
    string StoragePath,
    long Size);

/// <summary>
/// The access inputs of one episode for the current caller, resolved before any file is touched.
/// </summary>
/// <param name="Id">The episode identifier.</param>
/// <param name="CourseId">The course the episode ultimately belongs to.</param>
/// <param name="IsFree">Whether the episode is a free preview.</param>
/// <param name="CourseDisplayStatus">The public display status of the owning course.</param>
public sealed record EpisodeAccessInfoDto(
    long Id,
    long CourseId,
    bool IsFree,
    DisplayStatus CourseDisplayStatus);

/// <summary>
/// The attachment limits the admin upload modal shows; they mirror the server-side configuration.
/// </summary>
/// <param name="MaxAttachmentsPerEpisode">How many attachments one episode may hold.</param>
/// <param name="MaxAttachmentSizeBytes">The largest single attachment, in bytes.</param>
/// <param name="AllowedExtensions">The accepted file extensions, each with its leading dot.</param>
public sealed record EpisodeAttachmentLimitsDto(
    int MaxAttachmentsPerEpisode,
    long MaxAttachmentSizeBytes,
    IReadOnlyList<string> AllowedExtensions);
