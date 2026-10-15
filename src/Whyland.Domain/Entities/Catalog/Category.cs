
namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// A course category supporting a maximum hierarchy depth of three levels (s7.1).
/// </summary>
public class Category : BaseEntity<long>
{
    public string Name { get; set; } = string.Empty;

    public string LatinName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    /// <summary>
    /// Null for root categories. A category cannot be its own parent,
    /// and circular parenting is forbidden (s7.1.3).
    /// </summary>
    public long? ParentId { get; set; }

    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; } = [];

    /// <summary>
    /// Inactive categories must not be selectable as the category of new courses (s7.1.3).
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ICollection<CourseCategory> CourseCategories { get; set; } = [];
}
