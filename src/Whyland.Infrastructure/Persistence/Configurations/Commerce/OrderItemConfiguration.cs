namespace Whyland.Infrastructure.Persistence.Configurations.Commerce;

/// <summary>
/// Maps a single purchasable course inside an order.
/// </summary>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems", SchemaConstants.Commerce);

        #region Properties

        builder.Property(x => x.Quantity)
            .HasDefaultValue(1);

        // Money columns must be decimal-compatible, never float/double (s7.2.4)
        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.CourseId);

        #endregion

        #region Relations

        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
