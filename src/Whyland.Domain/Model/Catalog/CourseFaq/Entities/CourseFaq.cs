using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;
using Whyland.Domain.Model.Catalog.CourseFaq.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Entities;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<CourseFaqId>, ISoftDeletable
{
    #region Properties

    /// <summary>
    /// The course the FAQ item belongs to.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The course the FAQ item belongs to.
    /// </summary>
    public CourseEntity Course { get; private set; } = null!;

    /// <summary>
    /// The question text.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; items are shown in this order.
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

    #endregion

    #region Construction

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum allowed length of <see cref="Description"/>.</summary>
    public const int MaxDescriptionLength = 2000;

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private CourseFaq() { }

    /// <summary>
    /// Creates a new <see cref="CourseFaq"/> after trimming, validating and assigning the inputs.
    /// </summary>
    /// <param name="courseId">The course the question belongs to.</param>
    /// <param name="title">The question text. Will be trimmed.</param>
    /// <param name="description">The answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new question,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<CourseFaq> Create(
        CourseId courseId,
        string title,
        string description,
        int displayOrder)
    {
        var faq = new CourseFaq { CourseId = courseId };

        var applied = faq.Apply(title, description, displayOrder);
        return applied.IsFailure ? applied.Error : faq;
    }

    /// <summary>
    /// Updates the editable fields after trimming, validating and assigning them.
    /// </summary>
    /// <param name="title">The new question text. Will be trimmed.</param>
    /// <param name="description">The new answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string title, string description, int displayOrder)
        => Apply(title, description, displayOrder);

    /// <summary>
    /// Moves the question to a new position, as the reorder operation rewrites the weight of every
    /// item in the course's list.
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
    /// Marks the question as deleted so the global query filter hides it. The first deletion time
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
    /// <param name="title">The question text. Will be trimmed.</param>
    /// <param name="description">The answer text. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string title, string description, int displayOrder)
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
            return CourseFaqErrors.FieldRequired(nameof(Title));

        if (trimmedTitle.Length > MaxTitleLength)
            return CourseFaqErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (string.IsNullOrWhiteSpace(trimmedDescription))
            return CourseFaqErrors.FieldRequired(nameof(Description));

        if (trimmedDescription.Length > MaxDescriptionLength)
            return CourseFaqErrors.FieldTooLong(nameof(Description), MaxDescriptionLength);

        if (displayOrder < 0)
            return CourseFaqErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Title = trimmedTitle;
        Description = trimmedDescription;
        DisplayOrder = displayOrder;

        return Result.Success();
    }

    #endregion
}
