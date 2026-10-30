namespace Whyland.Infrastructure.Persistence.Configurations.Blog;

/// <summary>
/// Maps the hierarchical blog category entity.
/// </summary>
public sealed class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("BlogCategories", SchemaConstants.Blog);

        #region Properties

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(BlogCategory.MaxNameLength);

        builder.Property(x => x.LatinName)
            .IsRequired()
            .HasMaxLength(BlogCategory.MaxLatinNameLength);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(BlogCategory.MaxSlugLength);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.ParentId, x.DisplayOrder });

        #endregion

        #region Relations

        // Self-referencing hierarchy; a root category has a null Parent (s4.10)
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.BlogPosts)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
