
namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps the payable course order entity (s5.5).
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", SchemaConstants.Commerce);

        #region Properties

        // Money columns must be decimal-compatible, never float/double (s7.2.4)
        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CouponDiscountAmount)
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasDefaultValue(OrderStatus.Pending);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.CouponId);

        #endregion

        #region Relations

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Removing a coupon must not remove the historical orders that used it (s5.6)
        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion
    }
}
