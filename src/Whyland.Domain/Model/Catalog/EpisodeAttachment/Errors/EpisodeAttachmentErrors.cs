using Whyland.Domain.Common.Errors;

using EpisodeAttachmentEntity = Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities.EpisodeAttachment;

namespace Whyland.Domain.Model.Catalog.EpisodeAttachment.Errors;

/// <summary>
/// Every failure the <see cref="EpisodeAttachmentEntity"/> aggregate can report, including the ones
/// raised by the handlers that store, remove or read the file.
/// <para>
/// Each factory carries only the code, the category and the arguments the message needs. The wording
/// lives in the localization catalog, so a failure never embeds user input in a hand-written sentence.
/// </para>
/// </summary>
public static class EpisodeAttachmentErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    /// <param name="field">The name of the offending field.</param>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    /// <param name="field">The name of the offending field.</param>
    /// <param name="max">The maximum allowed length.</param>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The stored file is empty, so there is nothing to download.</summary>
    public static DomainError InvalidSize() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(InvalidSize), ErrorType.Validation, field: "Size");

    /// <summary>No attachment exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(long id) =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>The episode already holds the configured maximum number of attachments.</summary>
    /// <param name="episodeId">The episode whose limit was reached.</param>
    /// <param name="max">The configured maximum.</param>
    public static DomainError LimitReached(long episodeId, int max) =>
        DomainError.For<EpisodeAttachmentEntity>(
            nameof(LimitReached), ErrorType.Validation, field: null, ("EpisodeId", episodeId), ("Max", max));

    /// <summary>
    /// The caller may not download this attachment: attachments never inherit the free-preview
    /// exemption, so a protected one demands the course itself (s8.2.1).
    /// </summary>
    public static DomainError AccessDenied() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(AccessDenied), ErrorType.Forbidden);

    /// <summary>The uploaded file extension is not on the configured allow-list.</summary>
    /// <param name="extension">The extension that was refused.</param>
    public static DomainError InvalidExtension(string extension) =>
        DomainError.For<EpisodeAttachmentEntity>(
            nameof(InvalidExtension), ErrorType.Validation, field: "File", ("Extension", extension));

    /// <summary>The uploaded file is larger than the configured maximum.</summary>
    /// <param name="max">The configured maximum in bytes.</param>
    public static DomainError FileTooLarge(long max) =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(FileTooLarge), ErrorType.Validation, field: "File", ("Max", max));

    /// <summary>The attachment metadata could not be persisted (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The attachment could not be soft-deleted (persistence failure); masked behind a generic message.</summary>
    public static DomainError DeleteFailed() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(DeleteFailed), ErrorType.Unexpected);

    /// <summary>The attachments could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The file could not be stored (infrastructure failure); masked behind a generic message.</summary>
    public static DomainError UploadFailed() =>
        DomainError.For<EpisodeAttachmentEntity>(nameof(UploadFailed), ErrorType.Unexpected);
}
