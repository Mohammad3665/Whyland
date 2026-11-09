namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps a playable video episode inside a course section (s7.7).
/// </summary>
public sealed class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.ToTable("Episodes", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(Episode.MaxTitleLength);

        // Holds either a server-generated storage key or an absolute external URL, so the column is
        // sized for the larger of the two.
        builder.Property(x => x.Video)
            .HasMaxLength(Episode.MaxVideoUrlLength);

        #endregion

        #region Indexes

        // Episodes must be displayed by DisplayOrder (s7.7.2)
        builder.HasIndex(x => new { x.SectionId, x.DisplayOrder });

        #endregion

        #region Relations

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Episode)
            .HasForeignKey(x => x.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
