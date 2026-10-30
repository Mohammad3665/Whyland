namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps a per-course FAQ item shown on the course page (s7.10).
/// </summary>
public sealed class CourseFaqConfiguration : IEntityTypeConfiguration<CourseFaq>
{
    public void Configure(EntityTypeBuilder<CourseFaq> builder)
    {
        builder.ToTable("CourseFaqs", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(CourseFaq.MaxTitleLength);

        #endregion

        #region Indexes

        // FAQ items must be displayed by DisplayOrder (s7.10)
        builder.HasIndex(x => new { x.CourseId, x.DisplayOrder });

        #endregion
    }
}
