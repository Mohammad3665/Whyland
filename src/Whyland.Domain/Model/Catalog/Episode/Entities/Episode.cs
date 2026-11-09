using CourseSectionEntity = Whyland.Domain.Model.Catalog.CourseSection.Entities.CourseSection;
using EpisodeAttachmentEntity = Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities.EpisodeAttachment;
using Whyland.Domain.Model.Catalog.Episode.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.Episode.Entities;

/// <summary>
/// An individual educational lesson within a section (s7.7).
/// Note: per-user watch status is NOT stored here; it lives in a dedicated
/// per-user progress entity (s7.7.4).
/// </summary>
public class Episode : BaseEntity<EpisodeId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum allowed length of <see cref="Description"/>.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>Maximum allowed length of a stored video key.</summary>
    public const int MaxVideoLength = 512;

    /// <summary>
    /// Maximum allowed length of an external video link. The <see cref="Video"/> column is sized for
    /// this, because it holds either a server-generated key or an absolute URL.
    /// </summary>
    public const int MaxVideoUrlLength = 1024;

    /// <summary>Longest video duration the platform accepts for one episode.</summary>
    public static readonly TimeSpan MaxDuration = TimeSpan.FromHours(24);

    #endregion

    #region Properties

    /// <summary>
    /// The section the episode belongs to.
    /// </summary>
    public CourseSectionId SectionId { get; private set; }

    /// <summary>
    /// The section the episode belongs to.
    /// </summary>
    public CourseSectionEntity Section { get; private set; } = null!;

    /// <summary>
    /// The episode title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Optional short description of the episode's content.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Reference to the episode video: either a server-generated storage key of a protected MP4
    /// (served through the permission-checked endpoint, s8.1) or an absolute http(s) link to an
    /// externally hosted video. <see cref="HasExternalVideo"/> tells the two apart.
    /// </summary>
    public string? Video { get; private set; }

    /// <summary>
    /// Whether <see cref="Video"/> is an external link rather than a key inside protected storage.
    /// External videos are not stored, verified or streamed by this platform.
    /// </summary>
    public bool HasExternalVideo => IsExternalVideo(Video);

    /// <summary>
    /// Required when a video exists.
    /// </summary>
    public TimeSpan? Duration { get; private set; }

    /// <summary>
    /// A paid course can expose episodes as free previews (s7.7.3).
    /// Free preview access must not grant ownership of the whole course.
    /// </summary>
    public bool IsFree { get; private set; }

    /// <summary>
    /// Manual ordering weight; episodes are shown in ascending order within the section.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// The identifier of the user who performed the soft-delete operation.
    /// </summary>
    public UserId? DeletedBy { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted and must be excluded from ordinary queries.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// The UTC time at which the entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// The downloadable attachments of this episode.
    /// </summary>
    public ICollection<EpisodeAttachmentEntity> Attachments { get; } = [];

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private Episode() { }

    /// <summary>
    /// Creates a new <see cref="Episode"/> after trimming, validating and assigning the inputs.
    /// The video is attached separately once the uploaded file has passed validation (s8.3.2).
    /// </summary>
    /// <param name="sectionId">The section the episode belongs to.</param>
    /// <param name="title">The episode title. Will be trimmed.</param>
    /// <param name="description">The optional description. Will be trimmed; blank clears it.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <param name="isFree">Whether the episode is a free preview.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new episode,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Episode> Create(
        CourseSectionId sectionId,
        string title,
        string? description,
        int displayOrder,
        bool isFree)
    {
        var episode = new Episode { SectionId = sectionId };

        var applied = episode.Apply(title, description, displayOrder, isFree);
        return applied.IsFailure ? applied.Error : episode;
    }

    #endregion

    #region Behaviour

    /// <summary>
    /// Updates the editable fields after trimming, validating and assigning them. The video is not
    /// part of this: it changes through <see cref="SetVideo"/> or <see cref="ClearVideo"/>.
    /// </summary>
    /// <param name="title">The new title. Will be trimmed.</param>
    /// <param name="description">The new description. Will be trimmed; blank clears it.</param>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <param name="isFree">Whether the episode is a free preview.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string title, string? description, int displayOrder, bool isFree)
        => Apply(title, description, displayOrder, isFree);

    /// <summary>
    /// Moves the episode to a new position inside its section, as the reorder operation rewrites
    /// the weight of every episode in the list.
    /// </summary>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <returns>A success result, or a validation failure for a negative position.</returns>
    public Result SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            return EpisodeErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>
    /// Links the episode to a stored MP4 file. The key is generated by the storage layer, never sent
    /// by the browser, and the duration comes from the file itself (s7.7.1, s8.1.1).
    /// </summary>
    /// <param name="videoKey">The server-generated storage key of the video.</param>
    /// <param name="duration">The duration read from the MP4 header.</param>
    /// <returns>A success result, or a validation failure when the key or duration is not acceptable.</returns>
    public Result SetVideo(string videoKey, TimeSpan duration)
    {
        if (string.IsNullOrWhiteSpace(videoKey))
            return EpisodeErrors.VideoRequired();

        if (videoKey.Length > MaxVideoLength)
            return EpisodeErrors.FieldTooLong(nameof(Video), MaxVideoLength);

        if (duration <= TimeSpan.Zero || duration > MaxDuration)
            return EpisodeErrors.InvalidVideoDuration();

        Video = videoKey;
        Duration = duration;

        return Result.Success();
    }

    /// <summary>
    /// Links the episode to a video hosted elsewhere. The platform does not download or re-host it;
    /// the duration is optional metadata read from the provider and is left empty when the provider
    /// does not expose it. Playback is redirected only after the caller passes the same access check
    /// as a protected file (s7.7.1).
    /// </summary>
    /// <param name="videoUrl">The absolute http(s) link to the video.</param>
    /// <param name="duration">The duration reported by the provider, when available.</param>
    /// <returns>A success result, or a validation failure when the link or duration is not acceptable.</returns>
    public Result SetVideoUrl(string videoUrl, TimeSpan? duration = null)
    {
        var trimmed = videoUrl?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            return EpisodeErrors.VideoRequired();

        if (trimmed.Length > MaxVideoUrlLength)
            return EpisodeErrors.FieldTooLong(nameof(Video), MaxVideoUrlLength);

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return EpisodeErrors.InvalidVideoUrl();
        }

        if (duration is { } value && (value <= TimeSpan.Zero || value > MaxDuration))
            return EpisodeErrors.InvalidVideoDuration();

        Video = trimmed;
        Duration = duration;

        return Result.Success();
    }

    /// <summary>
    /// Whether a video value points outside this platform. Storage keys are relative and never carry
    /// a scheme, so a value with an http(s) scheme is always an external link.
    /// </summary>
    /// <param name="video">The value stored in <see cref="Video"/>.</param>
    public static bool IsExternalVideo(string? video)
        => !string.IsNullOrWhiteSpace(video)
            && Uri.TryCreate(video, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    /// <summary>
    /// Detaches the video (stored file or external link) so the episode becomes an attachment-only
    /// lesson. A stored file itself is removed by the storage layer after this is called.
    /// </summary>
    public void ClearVideo()
    {
        Video = null;
        Duration = null;
    }

    /// <summary>
    /// Marks the episode as deleted so the global query filter hides it. The first deletion time
    /// wins; re-deleting an already hidden row leaves its original stamp intact.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Trims, validates and assigns every editable value. Shared by create and update so both paths
    /// enforce exactly the same rules; on failure the instance is left unchanged.
    /// </summary>
    /// <param name="title">The episode title. Will be trimmed.</param>
    /// <param name="description">The optional description. Will be trimmed; blank clears it.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <param name="isFree">Whether the episode is a free preview.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string title, string? description, int displayOrder, bool isFree)
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
            return EpisodeErrors.FieldRequired(nameof(Title));

        if (trimmedTitle.Length > MaxTitleLength)
            return EpisodeErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (trimmedDescription?.Length > MaxDescriptionLength)
            return EpisodeErrors.FieldTooLong(nameof(Description), MaxDescriptionLength);

        if (displayOrder < 0)
            return EpisodeErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Title = trimmedTitle;
        Description = string.IsNullOrEmpty(trimmedDescription) ? null : trimmedDescription;
        DisplayOrder = displayOrder;
        IsFree = isFree;

        return Result.Success();
    }

    #endregion
}
