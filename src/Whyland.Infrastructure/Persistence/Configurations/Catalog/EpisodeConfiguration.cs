
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

public sealed class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.ToTable("Episodes", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Video)
            .HasMaxLength(512);

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
