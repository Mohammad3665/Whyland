namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps a section (chapter) inside a course that groups its episodes in display order (s7.6).
/// </summary>
public sealed class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
{
    public void Configure(EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("CourseSections", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(CourseSection.MaxTitleLength);

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
