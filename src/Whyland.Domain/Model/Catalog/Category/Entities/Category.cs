using Whyland.Domain.Model.Catalog.Category.Errors;
using Whyland.Shared.Results;

using CourseCategoryEntity = Whyland.Domain.Model.Catalog.CourseCategory.Entities.CourseCategory;

namespace Whyland.Domain.Model.Catalog.Category.Entities;

/// <summary>
/// A course category supporting a maximum hierarchy depth of three levels (s7.1).
/// </summary>
public class Category : BaseEntity<CategoryId>, ISoftDeletable
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="LatinName"/>; also the database column size.</summary>
    public const int MaxLatinNameLength = 200;

    /// <summary>Maximum allowed length of <see cref="Name"/>; also the database column size.</summary>
    public const int MaxNameLength = 200;

    /// <summary>
    /// The deepest level a category may sit on: a root is level one, so three means
    /// root → child → grandchild. The form, the save handlers and the listing all read
    /// the ceiling from here instead of repeating the number.
    /// </summary>
    public const int MaxDepth = 3;

    /// <summary>
    /// The widest a branch may get: how many children one category can hold. The ceiling
    /// is about other rows, so the create and the reparent handlers count them; this is the
    /// number they, the picker and the messages all read.
    /// </summary>
    public const int MaxChildren = 3;

    /// <summary>
    /// The level a root category sits on. The depth ceiling counts from here, so a level
    /// below it cannot exist and the filter picker refuses to search for one.
    /// </summary>
    public const int MinDepth = 1;

    #endregion

    #region Properties

    /// <summary>
    /// The category name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Latin equivalent of the name, used to generate URL-friendly slugs.
    /// </summary>
    public string LatinName { get; private set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; counted between siblings, so the table shows each branch
    /// in this order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Null for root categories. A category cannot be its own parent,
    /// and circular parenting is forbidden (s7.1.3).
    /// </summary>
    public CategoryId? ParentId { get; private set; }

    /// <summary>
    /// Inactive categories must not be selectable as the category of new courses (s7.1.3).
    /// Switching one off also switches off its whole branch, so a disabled parent never
    /// leaves live children hanging above it.
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

    /// <summary>
    /// The parent category, when this is a nested category.
    /// </summary>
    public Category? Parent { get; private set; }

    /// <summary>
    /// The child categories nested under this one.
    /// </summary>
    public ICollection<Category> Children { get; } = [];

    /// <summary>
    /// The categories this course belongs to.
    /// </summary>
    public ICollection<CourseCategoryEntity> CourseCategories { get; } = [];

    #endregion

    #region Construction

    /// <summary>
    /// Parameterless constructor required by ORMs (e.g. EF Core).
    /// Not intended for direct use — create instances via <see cref="Create"/>.
    /// </summary>
    private Category() { }

    /// <summary>
    /// Creates a new <see cref="Category"/> after trimming, validating and assigning the inputs.
    /// Whether the chosen parent exists, and whether hanging under it stays inside the
    /// depth ceiling, are questions about other rows — the handlers answer those.
    /// </summary>
    /// <param name="name">The category name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name used for the slug. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight inside its sibling group; zero or greater.</param>
    /// <param name="parentId">The parent category, or null for a root.</param>
    /// <param name="isActive">Whether the category is visible on the site.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the new category,
    /// or a failure describing the first violated invariant.
    /// </returns>
    public static Result<Category> Create(
        string name,
        string latinName,
        int displayOrder,
        CategoryId? parentId,
        bool isActive)
    {
        var category = new Category();

        var applied = category.Apply(name, latinName, displayOrder, parentId, isActive);
        return applied.IsFailure ? applied.Error : category;
    }

    /// <summary>
    /// Updates the mutable fields of an existing category after trimming, validating and assigning them.
    /// The display order and the parent are not part of this: the first belongs to drag &amp; drop
    /// (<see cref="SetDisplayOrder"/>) and the second to <see cref="SetParent"/>.
    /// </summary>
    /// <param name="name">The new name. Will be trimmed.</param>
    /// <param name="latinName">The new Latin name. Will be trimmed.</param>
    /// <param name="isActive">Whether the category should be visible on the site.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the update succeeds,
    /// or a failure describing the first violated invariant. On failure, the instance is left unchanged.
    /// </returns>
    public Result Update(string name, string latinName, bool isActive)
        => Apply(name, latinName, DisplayOrder, ParentId, isActive);

    /// <summary>
    /// Moves the category to a new position inside its sibling group, as both the form and the
    /// drag &amp; drop reorder rewrite the weight of every row in the group.
    /// </summary>
    /// <param name="displayOrder">The new ordering weight; must be zero or greater.</param>
    public Result SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            return CategoryErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        DisplayOrder = displayOrder;
        return Result.Success();
    }

    /// <summary>
    /// Hangs the category under a new parent (null moves it to the root level). The caller has
    /// already proved the parent exists, is not a descendant of this row, and leaves the branch
    /// inside <see cref="MaxDepth"/>.
    /// </summary>
    /// <param name="parentId">The new parent identifier, or null for the root level.</param>
    public void SetParent(CategoryId? parentId) => ParentId = parentId;

    /// <summary>
    /// Switches the category off without touching its name — used when a disabled parent
    /// cascades down to its branch.
    /// </summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Marks the category as deleted so the global query filter hides it, keeping the row for the
    /// courses still linked to it. The first deletion time wins; re-deleting an already hidden row
    /// leaves its original stamp intact.
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt ??= DateTime.UtcNow;
    }

    /// <summary>
    /// Trims, validates and assigns every editable value. Shared by create and update so both paths
    /// enforce exactly the same rules; on failure the instance is left unchanged. Update hands in
    /// the current order and parent, so this method never moves a row on its own.
    /// </summary>
    /// <param name="name">The category name. Will be trimmed.</param>
    /// <param name="latinName">The Latin name used for the slug. Will be trimmed.</param>
    /// <param name="displayOrder">The ordering weight inside its sibling group; zero or greater.</param>
    /// <param name="parentId">The parent category, or null for a root.</param>
    /// <param name="isActive">Whether the category is visible on the site.</param>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(
        string name,
        string latinName,
        int displayOrder,
        CategoryId? parentId,
        bool isActive)
    {
        var trimmedName = name?.Trim();
        var trimmedLatinName = latinName?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedName))
            return CategoryErrors.FieldRequired(nameof(Name));

        if (trimmedName.Length > MaxNameLength)
            return CategoryErrors.FieldTooLong(nameof(Name), MaxNameLength);

        if (string.IsNullOrWhiteSpace(trimmedLatinName))
            return CategoryErrors.FieldRequired(nameof(LatinName));

        if (trimmedLatinName.Length > MaxLatinNameLength)
            return CategoryErrors.FieldTooLong(nameof(LatinName), MaxLatinNameLength);

        if (displayOrder < 0)
            return CategoryErrors.GreaterThanOrEqualToZero(nameof(DisplayOrder));

        Name = trimmedName;
        LatinName = trimmedLatinName;
        DisplayOrder = displayOrder;
        ParentId = parentId;
        IsActive = isActive;

        return Result.Success();
    }

    #endregion
}
