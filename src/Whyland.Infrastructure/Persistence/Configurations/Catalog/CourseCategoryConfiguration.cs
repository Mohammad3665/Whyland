
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps the many-to-many link between courses and categories (s7.4).
/// </summary>
public sealed class CourseCategoryConfiguration : IEntityTypeConfiguration<CourseCategory>
{
    public void Configure(EntityTypeBuilder<CourseCategory> builder)
    {
        builder.ToTable("CourseCategories", SchemaConstants.Catalog);

        #region Keys

        // Duplicate course/category relations must not be allowed (s7.4)
        builder.HasKey(x => new { x.CourseId, x.CategoryId });

        #endregion

        #region Relations

        builder.HasOne(x => x.Category)
            .WithMany(x => x.CourseCategories)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
