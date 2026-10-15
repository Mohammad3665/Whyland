
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LatinName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ShortDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.MainImage)
            .HasMaxLength(512);

        builder.Property(x => x.IntroductionVideo)
            .HasMaxLength(512);

        // Decimal-compatible money type; float/double are forbidden for money (s7.2.4)
        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.DiscountValue)
            .HasPrecision(18, 2);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => x.DisplayStatus);

        builder.HasIndex(x => x.IsFeatured);

        builder.HasIndex(x => x.IsAmazing);

        #endregion

        #region Relations

        builder.HasMany(x => x.CourseCategories)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CourseInstructors)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Sections)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CourseFaqs)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
