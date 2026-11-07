
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;
using Whyland.Domain.Model.Catalog.CourseFaq.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.CourseFaq.Entities;

/// <summary>
/// A FAQ item attached to a specific course (s7.10); separate from public FAQs (s12.2).
/// </summary>
public class CourseFaq : BaseEntity<CourseFaqId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>Maximum allowed length of <see cref="Description"/>.</summary>
    public const int MaxDescriptionLength = 2000;

    #endregion
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

    #region Construction

    /// <summary>Parameterless constructor required by ORMs (e.g. EF Core).</summary>
    private CourseFaq() { }

    /// <summary>Creates a course FAQ after trimming and validating its question and answer.</summary>
    public static Result<CourseFaq> Create(
        CourseId courseId,
        string title,
        string description,
        int displayOrder)
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        var validation = Validate(trimmedTitle, trimmedDescription, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        return new CourseFaq
        {
            CourseId = courseId,
            Title = trimmedTitle!,
            Description = trimmedDescription!,
            DisplayOrder = displayOrder,
        };
    }

    /// <summary>Updates the editable fields after trimming and validating them.</summary>
    public Result Update(string title, string description, int displayOrder)
    {
        var trimmedTitle = title?.Trim();
        var trimmedDescription = description?.Trim();

        var validation = Validate(trimmedTitle, trimmedDescription, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        Title = trimmedTitle!;
        Description = trimmedDescription!;
        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>Moves the FAQ to a new position during reordering.</summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "A position cannot be negative.");

        DisplayOrder = displayOrder;
    }

    /// <summary>Marks the FAQ as deleted; the first deletion time wins.</summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    private static Result Validate(string? title, string? description, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            return CourseFaqErrors.FieldRequired(nameof(Title));

        if (title.Length > MaxTitleLength)
            return CourseFaqErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (string.IsNullOrWhiteSpace(description))
            return CourseFaqErrors.FieldRequired(nameof(Description));

        if (description.Length > MaxDescriptionLength)
            return CourseFaqErrors.FieldTooLong(nameof(Description), MaxDescriptionLength);

        if (displayOrder < 0)
            return CourseFaqErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        return Result.Success();
    }

    #endregion
}
