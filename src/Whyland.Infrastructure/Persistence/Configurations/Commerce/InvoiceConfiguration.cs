
namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps the printable purchase invoice with its immutable customer/money snapshots (s5.8).
/// </summary>
public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices", SchemaConstants.Commerce);

        #region Properties

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CustomerPhone)
            .HasMaxLength(20);

        builder.Property(x => x.CustomerEmail)
            .HasMaxLength(256);

        // Money columns must be decimal-compatible, never float/double (s7.2.4)
        builder.Property(x => x.Subtotal)
            .HasPrecision(18, 2);

        builder.Property(x => x.Discount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CouponCode)
            .HasMaxLength(50);

        builder.Property(x => x.CouponDiscount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2);

        builder.Property(x => x.FinalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PaymentStatus)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        #endregion

        #region Indexes

        // An invoice number must be unique across the whole system (s5.8)
        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.PaymentId);

        #endregion

        #region Relations

        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
