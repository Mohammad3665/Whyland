
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;
using EpisodeEntity = Whyland.Domain.Model.Catalog.Episode.Entities.Episode;
using Whyland.Domain.Model.Catalog.CourseSection.Errors;
using Whyland.Shared.Results;

namespace Whyland.Domain.Model.Catalog.CourseSection.Entities;

/// <summary>
/// A logical grouping of episodes within a course (s7.6).
/// </summary>
public class CourseSection : BaseEntity<CourseSectionId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    #endregion
    /// <summary>
    /// The course the section belongs to.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The course the section belongs to.
    /// </summary>
    public CourseEntity Course { get; private set; } = null!;

    /// <summary>
    /// The section title shown in the course curriculum.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Sections must be displayed in ascending DisplayOrder (s7.6).
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
    /// The episodes grouped within this section.
    /// </summary>
    public ICollection<EpisodeEntity> Episodes { get; } = [];

    #region Construction

    /// <summary>Parameterless constructor required by ORMs (e.g. EF Core).</summary>
    private CourseSection() { }

    /// <summary>Creates a new section after trimming and validating its title and order.</summary>
    public static Result<CourseSection> Create(CourseId courseId, string title, int displayOrder)
    {
        var trimmedTitle = title?.Trim();

        var validation = Validate(trimmedTitle, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        return new CourseSection
        {
            CourseId = courseId,
            Title = trimmedTitle!,
            DisplayOrder = displayOrder,
        };
    }

    /// <summary>Updates the editable fields after trimming and validating them.</summary>
    public Result Update(string title, int displayOrder)
    {
        var trimmedTitle = title?.Trim();

        var validation = Validate(trimmedTitle, displayOrder);
        if (validation.IsFailure)
            return validation.Error;

        Title = trimmedTitle!;
        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>Moves the section to a new position during reordering.</summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder), displayOrder, "A position cannot be negative.");

        DisplayOrder = displayOrder;
    }

    /// <summary>Marks the section as deleted; the first deletion time wins.</summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    private static Result Validate(string? title, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(title))
            return CourseSectionErrors.FieldRequired(nameof(Title));

        if (title.Length > MaxTitleLength)
            return CourseSectionErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (displayOrder < 0)
            return CourseSectionErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        return Result.Success();
    }

    #endregion
}
