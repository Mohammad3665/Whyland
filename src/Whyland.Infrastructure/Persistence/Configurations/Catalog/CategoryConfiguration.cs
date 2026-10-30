namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps the hierarchical course category entity (s7.1).
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(Category.MaxNameLength);

        builder.Property(x => x.LatinName)
            .IsRequired()
            .HasMaxLength(Category.MaxLatinNameLength);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        #endregion

        #region Relations

        // Deleting a category referenced as a parent must not orphan its children (s7.1.3)
        builder.HasOne(x => x.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
