using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;
using Whyland.Domain.Model.Catalog.EpisodeAttachment.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.EpisodeAttachment.Entities;

/// <summary>
/// A downloadable attachment of an episode (s7.8).
/// Count/size/extension limits are configuration-driven and enforced server-side;
/// file metadata is persisted while the binary lives in protected wwwroot storage (s7.8, s8.1).
/// </summary>
public class EpisodeAttachment : BaseEntity<EpisodeAttachmentId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="ContentType"/>; also the database column size.</summary>
    public const int MaxContentTypeLength = 128;

    /// <summary>Maximum allowed length of <see cref="FileName"/>; also the database column size.</summary>
    public const int MaxFileNameLength = 256;

    /// <summary>Maximum allowed length of <see cref="StoragePath"/>; also the database column size.</summary>
    public const int MaxStoragePathLength = 512;

    #endregion

    #region Properties

    /// <summary>
    /// The episode the attachment belongs to.
    /// </summary>
    public EpisodeId EpisodeId { get; private set; }

    /// <summary>
    /// The episode the attachment belongs to.
    /// </summary>
    public EpisodeEntity Episode { get; private set; } = null!;

    /// <summary>
    /// The display/download file name shown to the user.
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// Server-managed storage reference under protected wwwroot. Must never expose
    /// physical paths or allow arbitrary path traversal (s8.1.1, s8.5).
    /// </summary>
    public string StoragePath { get; private set; } = string.Empty;

    /// <summary>
    /// The MIME content type used when serving the download.
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Size in bytes; validated against the configured maximum.
    /// </summary>
    public long Size { get; private set; }

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

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private EpisodeAttachment() { }

    /// <summary>
    /// Creates a new attachment after validating the metadata produced by the storage layer.
    /// The storage key is generated server-side and is never the browser's file name.
    /// </summary>
    /// <param name="episodeId">The episode the attachment belongs to.</param>
    /// <param name="fileName">The sanitized display name of the file.</param>
    /// <param name="storagePath">The server-generated storage key.</param>
    /// <param name="contentType">The MIME type resolved from the validated file.</param>
    /// <param name="size">The stored file size in bytes.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new attachment,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<EpisodeAttachment> Create(
        EpisodeId episodeId,
        string fileName,
        string storagePath,
        string contentType,
        long size)
    {
        var attachment = new EpisodeAttachment { EpisodeId = episodeId };

        var applied = attachment.Apply(fileName, storagePath, contentType, size);
        return applied.IsFailure ? applied.Error : attachment;
    }

    #endregion

    #region Behaviour

    /// <summary>
    /// Marks the attachment as deleted so the global query filter hides it. The file itself is removed
    /// by the storage layer before this is called.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Validates and assigns the file metadata. Shared with create so the rules live in one place;
    /// on failure the instance is left unchanged.
    /// </summary>
    /// <param name="fileName">The sanitized display name of the file.</param>
    /// <param name="storagePath">The server-generated storage key.</param>
    /// <param name="contentType">The MIME type resolved from the validated file.</param>
    /// <param name="size">The stored file size in bytes.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string fileName, string storagePath, string contentType, long size)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return EpisodeAttachmentErrors.FieldRequired(nameof(FileName));

        if (fileName.Length > MaxFileNameLength)
            return EpisodeAttachmentErrors.FieldTooLong(nameof(FileName), MaxFileNameLength);

        if (string.IsNullOrWhiteSpace(storagePath))
            return EpisodeAttachmentErrors.FieldRequired(nameof(StoragePath));

        if (storagePath.Length > MaxStoragePathLength)
            return EpisodeAttachmentErrors.FieldTooLong(nameof(StoragePath), MaxStoragePathLength);

        if (string.IsNullOrWhiteSpace(contentType))
            return EpisodeAttachmentErrors.FieldRequired(nameof(ContentType));

        if (contentType.Length > MaxContentTypeLength)
            return EpisodeAttachmentErrors.FieldTooLong(nameof(ContentType), MaxContentTypeLength);

        if (size <= 0)
            return EpisodeAttachmentErrors.InvalidSize();

        FileName = fileName;
        StoragePath = storagePath;
        ContentType = contentType;
        Size = size;

        return Result.Success();
    }

    #endregion
}
