
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

public sealed class EpisodeAttachmentConfiguration : IEntityTypeConfiguration<EpisodeAttachment>
{
    public void Configure(EntityTypeBuilder<EpisodeAttachment> builder)
    {
        builder.ToTable("EpisodeAttachments", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        #endregion
    }
}
