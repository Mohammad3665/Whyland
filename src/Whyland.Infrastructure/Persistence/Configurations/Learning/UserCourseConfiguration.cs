namespace Whyland.Infrastructure.Persistence.Configurations.Learning;

/// <summary>
/// Maps the purchase record that grants a user permanent access to a course (s4.9).
/// </summary>
public sealed class UserCourseConfiguration : IEntityTypeConfiguration<UserCourse>
{
    public void Configure(EntityTypeBuilder<UserCourse> builder)
    {
        builder.ToTable("UserCourses", SchemaConstants.Learning);

        #region Keys

        // A user owns each course exactly once; re-purchasing is rejected (s4.9)
        builder.HasKey(x => new { x.UserId, x.CourseId });

        #endregion

        #region Properties

        builder.Property(x => x.PurchasedAt);

        #endregion

        #region Indexes

        builder.HasIndex(x => x.OrderId);

        #endregion

        #region Relations

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // The course must remain available while purchase records reference it (s4.9)
        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting the source order must not silently revoke course access
        builder.HasOne(x => x.Order)
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
