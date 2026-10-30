using Whyland.Domain.Entities.Settings;

namespace Whyland.Infrastructure.Persistence.Configurations.Settings;

/// <summary>
/// Maps the editable site configuration entry with its typed value (s4.11).
/// </summary>
public sealed class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> builder)
    {
        builder.ToTable("SiteSettings", SchemaConstants.Settings);

        #region Properties

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(SiteSetting.MaxKeyLength);

        builder.Property(x => x.Value)
            .HasMaxLength(SiteSetting.MaxValueLength);

        builder.Property(x => x.Group)
            .IsRequired();

        builder.Property(x => x.DataType)
            .IsRequired();

        builder.Property(x => x.IsPublic)
            .HasDefaultValue(false);

        builder.Property(x => x.Description)
            .HasMaxLength(SiteSetting.MaxDescriptionLength);

        #endregion

        #region Indexes

        // Setting keys must be unique; lookups always go through the key (s4.11)
        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.HasIndex(x => x.Group);

        #endregion
    }
}
