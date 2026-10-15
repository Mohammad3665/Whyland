
namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps a payment attempt against an order, including its gateway tracking number (s5.7).
/// </summary>
public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", SchemaConstants.Commerce);

        #region Properties

        builder.Property(x => x.PaymentMethod)
            .IsRequired();

        // Money columns must be decimal-compatible, never float/double (s7.2.4)
        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.Status);

        // The gateway reference must be unique when present (s5.7)
        builder.HasIndex(x => x.TrackingNumber)
            .IsUnique()
            .HasFilter($"\"{nameof(Payment.TrackingNumber)}\" IS NOT NULL");

        #endregion

        #region Relations

        // Configured on the Order side (OrderConfiguration)

        #endregion
    }
}
