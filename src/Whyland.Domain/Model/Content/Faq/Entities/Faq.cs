using Whyland.Domain.Model.Content.Faq.Errors;
using Whyland.Domain.Model.Content.Faq.Identifiers;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Content.Faq.Entities;

/// <summary>
/// A standalone FAQ item shown in the general-purpose FAQ section (s4.12).
/// </summary>
public class Faq : BaseEntity<FaqId>, ISoftDeletable
{
    #region Properties

    /// <summary>
    /// The question text.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight (s4.12).
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Inactive FAQs are hidden from the site but kept for editing.
    /// </summary>
    public bool IsActive { get; private set; }

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

    /// <summary>Maximum allowed length of <see cref="Title"/>.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum allowed length of <see cref="Description"/>.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private Faq() { }

    /// <summary>
    /// Creates a new <see cref="Faq"/> after trimming, validating and assigning the inputs.
    /// </summary>
    /// <param name="title">The question text. Will be trimmed.</param>
    /// <param name="description">The answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <param name="isActive">Whether the FAQ is visible on the site.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new FAQ,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Faq> Create(string title, string description, int displayOrder, bool isActive)
    {
        var faq = new Faq();

        var applied = faq.Apply(title, description, displayOrder, isActive);
        return applied.IsFailure ? applied.Error : faq;
    }

    /// <summary>
    /// Updates the FAQ's mutable fields after trimming, validating and assigning them.
    /// </summary>
    /// <param name="title">The new question text. Will be trimmed.</param>
    /// <param name="description">The new answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <param name="isActive">Whether the FAQ should be visible on the site.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string title, string description, int displayOrder, bool isActive)
        => Apply(title, description, displayOrder, isActive);

    /// <summary>
    /// Moves the FAQ to a new position, as the reorder operation rewrites the weight of every item in the list.
    /// </summary>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the position is negative.</exception>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "A position cannot be negative.");

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Marks the FAQ as deleted so the global query filter hides it. The first deletion time wins;
    /// re-deleting an already hidden row leaves its original stamp intact.
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
    /// <param name="title">The question text. Will be trimmed.</param>
    /// <param name="description">The answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <param name="isActive">Whether the FAQ is visible on the site.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string title, string description, int displayOrder, bool isActive)
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
            return FaqErrors.FieldRequired(nameof(Title));

        if (trimmedTitle.Length > MaxTitleLength)
            return FaqErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (string.IsNullOrWhiteSpace(trimmedDescription))
            return FaqErrors.FieldRequired(nameof(Description));

        if (trimmedDescription.Length > MaxDescriptionLength)
            return FaqErrors.FieldTooLong(nameof(Description), MaxDescriptionLength);

        if (displayOrder < 0)
            return FaqErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Title = trimmedTitle;
        Description = trimmedDescription;
        DisplayOrder = displayOrder;
        IsActive = isActive;

        return Result.Success();
    }

    #endregion
}