namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps the discount coupon entity and its usage rules (s5.6).
/// </summary>
public sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons", SchemaConstants.Commerce);

        #region Properties

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(Coupon.MaxCodeLength);

        builder.Property(x => x.Type)
            .IsRequired();

        // Percentage and amount coupons share the same column; precision covers both (s5.6)
        builder.Property(x => x.Value)
            .HasPrecision(18, 2);

        builder.Property(x => x.MinOrderAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.UsedCount)
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        #endregion

        #region Indexes

        // A coupon code must be unique and is looked up case-sensitively (s5.6)
        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.HasIndex(x => x.StartDate);

        builder.HasIndex(x => x.EndDate);

        #endregion

        #region Relations

        // Configured on the Order side (OrderConfiguration)

        #endregion
    }
}
