
namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps a single invoiced course line with its price snapshot.
/// </summary>
public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems", SchemaConstants.Commerce);

        #region Properties

        builder.Property(x => x.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Quantity)
            .HasDefaultValue(1);

        // Money columns must be decimal-compatible, never float/double (s7.2.4)
        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.Discount)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.InvoiceId);

        builder.HasIndex(x => x.CourseId);

        #endregion

        #region Relations

        // Line items preserve the CourseId snapshot even though the course is not navigable
        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
