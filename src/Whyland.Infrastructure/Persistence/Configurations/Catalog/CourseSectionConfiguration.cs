
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

public sealed class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("CourseSections", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        #endregion

        #region Indexes

        // Sections must be displayed in ascending DisplayOrder (s7.6)
        builder.HasIndex(x => new { x.CourseId, x.DisplayOrder });

        #endregion

        #region Relations

        builder.HasMany(x => x.Episodes)
            .WithOne(x => x.Section)
            .HasForeignKey(x => x.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
