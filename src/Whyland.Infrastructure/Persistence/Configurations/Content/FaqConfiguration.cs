using Whyland.Domain.Entities.Content;

namespace Whyland.Infrastructure.Persistence.Configurations.Content;

/// <summary>
/// Maps the standalone FAQ item shown in the general-purpose FAQ section.
/// </summary>
public sealed class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs", SchemaConstants.Content);

        #region Properties

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        #endregion

        #region Indexes

        // FAQs are always rendered in DisplayOrder (s4.12)
        builder.HasIndex(x => x.DisplayOrder);

        #endregion
    }
}
