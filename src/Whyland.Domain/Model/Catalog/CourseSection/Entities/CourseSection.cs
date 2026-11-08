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
    #region Properties

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

    #endregion

    #region Construction

    /// <summary>Maximum allowed length of <see cref="Title"/>; also the database column size.</summary>
    public const int MaxTitleLength = 200;

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private CourseSection() { }

    /// <summary>
    /// Creates a new <see cref="CourseSection"/> after trimming, validating and assigning the inputs.
    /// </summary>
    /// <param name="courseId">The course the section belongs to.</param>
    /// <param name="title">The section title. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new section,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<CourseSection> Create(CourseId courseId, string title, int displayOrder)
    {
        var section = new CourseSection { CourseId = courseId };

        var applied = section.Apply(title, displayOrder);
        return applied.IsFailure ? applied.Error : section;
    }

    /// <summary>
    /// Updates the editable fields after trimming, validating and assigning them.
    /// </summary>
    /// <param name="title">The new section title. Will be trimmed.</param>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string title, int displayOrder)
        => Apply(title, displayOrder);

    /// <summary>
    /// Moves the section to a new position, as the reorder operation rewrites the weight of every
    /// section in the course.
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
    /// Marks the section as deleted so the global query filter hides it. The first deletion time
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
    /// <param name="title">The section title. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight; must be zero or greater.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(string title, int displayOrder)
    {
        var trimmedTitle = title?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedTitle))
            return CourseSectionErrors.FieldRequired(nameof(Title));

        if (trimmedTitle.Length > MaxTitleLength)
            return CourseSectionErrors.FieldTooLong(nameof(Title), MaxTitleLength);

        if (displayOrder < 0)
            return CourseSectionErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Title = trimmedTitle;
        DisplayOrder = displayOrder;

        return Result.Success();
    }

    #endregion
}
