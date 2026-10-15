
namespace Whyland.Infrastructure.Persistence.Configurations.Blog;

/// <summary>
/// Maps the blog post entity with author and display metadata.
/// </summary>
public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts", SchemaConstants.Blog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Image)
            .HasMaxLength(512);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.DisplayStatus);

        builder.HasIndex(x => new { x.CategoryId, x.DisplayOrder });

        #endregion

        #region Relations

        // The authoring user must stay intact while posts reference it
        builder.HasOne(x => x.Author)
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configured on the BlogCategory side (BlogCategoryConfiguration)

        #endregion
    }
}
