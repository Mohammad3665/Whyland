using Whyland.Domain.Common.Errors;

using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;

namespace Whyland.Domain.Model.Catalog.Episode.Errors;

/// <summary>
/// Every failure the <see cref="EpisodeEntity"/> aggregate can report, including the ones raised by
/// the handlers that save, upload media for, or read it.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs. The wording
/// lives in the localization catalog, so a failure never embeds user input in a hand-written sentence.
/// </para>
/// </summary>
public static class EpisodeErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<EpisodeEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<EpisodeEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The specified field carries a negative value where only zero or above is allowed.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError GreaterThanOrEqualToZero(string field) =>
        DomainError.For<EpisodeEntity>(nameof(GreaterThanOrEqualToZero), ErrorType.Validation, field: field);

    /// <summary>No episode exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(long id) =>
        DomainError.For<EpisodeEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>The episode still carries downloadable attachments, so deleting it would orphan them.</summary>
    /// <param name="id">The identifier of the episode that was refused.</param>
    public static DomainError HasAttachments(long id) =>
        DomainError.For<EpisodeEntity>(nameof(HasAttachments), ErrorType.Validation, field: null, ("Id", id));

    /// <summary>The video key handed to the aggregate is missing.</summary>
    public static DomainError VideoRequired() =>
        DomainError.For<EpisodeEntity>(nameof(VideoRequired), ErrorType.Validation, field: "Video");

    /// <summary>The video container reported no usable duration, or one outside the accepted range.</summary>
    public static DomainError InvalidVideoDuration() =>
        DomainError.For<EpisodeEntity>(nameof(InvalidVideoDuration), ErrorType.Validation, field: "Duration");

    /// <summary>The external video link is not an absolute http(s) URL.</summary>
    public static DomainError InvalidVideoUrl() =>
        DomainError.For<EpisodeEntity>(nameof(InvalidVideoUrl), ErrorType.Validation, field: "Video");

    /// <summary>The episode could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<EpisodeEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The episode could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<EpisodeEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The episode could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError SoftDeleteFailed() =>
        DomainError.For<EpisodeEntity>(nameof(SoftDeleteFailed), ErrorType.Unexpected);

    /// <summary>The episodes could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<EpisodeEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The new display order could not be persisted (persistence failure); masked behind a generic message.</summary>
    public static DomainError ReorderFailed() =>
        DomainError.For<EpisodeEntity>(nameof(ReorderFailed), ErrorType.Unexpected);

    /// <summary>
    /// The caller may not watch this episode: an unauthenticated caller for a protected video, an
    /// account without the course (s7.7.3) or content-management permission.
    /// </summary>
    public static DomainError AccessDenied() =>
        DomainError.For<EpisodeEntity>(nameof(AccessDenied), ErrorType.Forbidden);

    /// <summary>The uploaded video was rejected by the storage validation.</summary>
    /// <param name="reason">The reason code reported by the storage layer.</param>
    public static DomainError VideoRejected(string reason) =>
        DomainError.For<EpisodeEntity>(nameof(VideoRejected), ErrorType.Validation, field: "Video", ("Reason", reason));

    /// <summary>The video file could not be stored (infrastructure failure); masked behind a generic message.</summary>
    public static DomainError VideoUploadFailed() =>
        DomainError.For<EpisodeEntity>(nameof(VideoUploadFailed), ErrorType.Unexpected);

    /// <summary>The stored video could not be removed (infrastructure failure); masked behind a generic message.</summary>
    public static DomainError VideoDeleteFailed() =>
        DomainError.For<EpisodeEntity>(nameof(VideoDeleteFailed), ErrorType.Unexpected);
}
