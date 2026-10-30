using Whyland.Domain.Entities.Content.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Entities.Content;

/// <summary>
/// A standalone FAQ item shown in the general-purpose FAQ section (s4.12).
/// </summary>
public class Faq : BaseEntity<long>, ISoftDeletable
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
    /// Initializes a validated <see cref="Faq"/> instance.
    /// Assumes the inputs have already passed <see cref="Validate"/>.
    /// </summary>
    private Faq(string title, string description, int displayOrder, bool isActive)
    {
        Title = title;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;
    }

    /// <summary>
    /// Creates a new <see cref="Faq"/> after trimming and validating the inputs.
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
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        var validation = Validate(trimmedTitle, trimmedDescription, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        return new Faq(trimmedTitle!, trimmedDescription!, displayOrder, isActive);
    }

    /// <summary>
    /// Updates the FAQ's mutable fields after trimming and validating the inputs.
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
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        var validation = Validate(trimmedTitle, trimmedDescription, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        Title = trimmedTitle!;
        Description = trimmedDescription!;
        DisplayOrder = displayOrder;
        IsActive = isActive;

        return Result.Success();
    }

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
    /// Validates the trimmed inputs against the FAQ's invariants.
    /// Shared by <see cref="Create"/> and <see cref="Update"/> so both paths enforce
    /// exactly the same rules. Returns the first violation encountered.
    /// </summary>
    /// <param name="title">The trimmed title to validate.</param>
    /// <param name="description">The trimmed description to validate.</param>
    /// <param name="displayOrder">The ordering weight to validate.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private static Result Validate(string? title, string? description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            return FaqErrors.FieldRequired(nameof(Title));

        if (title.Length > MaxTitleLength)
            return FaqErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (string.IsNullOrWhiteSpace(description))
            return FaqErrors.FieldRequired(nameof(Description));

        if (description.Length > MaxDescriptionLength)
            return FaqErrors.FieldTooLong(nameof(Description), MaxDescriptionLength);

        if (displayOrder < 0)
            return FaqErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        return Result.Success();
    }

    #endregion
}