namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps the downloadable file attachment that belongs to a course episode (s7.7.4).
/// </summary>
public sealed class EpisodeAttachmentConfiguration : IEntityTypeConfiguration<EpisodeAttachment>
{
    public void Configure(EntityTypeBuilder<EpisodeAttachment> builder)
    {
        builder.ToTable("EpisodeAttachments", SchemaConstants.Catalog);

        #region Properties

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(EpisodeAttachment.MaxFileNameLength);

        builder.Property(x => x.StoragePath)
            .IsRequired()
            .HasMaxLength(EpisodeAttachment.MaxStoragePathLength);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(EpisodeAttachment.MaxContentTypeLength);

        #endregion
    }
}
