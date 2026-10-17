namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A course category supporting a maximum hierarchy depth of three levels (s7.1).
/// </summary>
public class Category : BaseEntity<long>
{
    /// <summary>
    /// The category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Latin equivalent of the name, used to generate URL-friendly slugs.
    /// </summary>
    public string LatinName { get; set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight; categories are shown in this order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Null for root categories. A category cannot be its own parent,
    /// and circular parenting is forbidden (s7.1.3).
    /// </summary>
    public long? ParentId { get; set; }

    /// <summary>
    /// The parent category, when this is a nested category.
    /// </summary>
    public Category? Parent { get; set; }

    /// <summary>
    /// The child categories nested under this one.
    /// </summary>
    public ICollection<Category> Children { get; set; } = [];

    /// <summary>
    /// Inactive categories must not be selectable as the category of new courses (s7.1.3).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The categories this course belongs to.
    /// </summary>
    public ICollection<CourseCategory> CourseCategories { get; set; } = [];
}
