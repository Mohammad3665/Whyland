using Whyland.Domain.Entities.Interactions;

namespace Whyland.Infrastructure.Persistence.Configurations.Interactions;

/// <summary>
/// Maps the favorite ("wishlist") link between a user and a course.
/// </summary>
public sealed class FavoriteCourseConfiguration : IEntityTypeConfiguration<FavoriteCourse>
{
    public void Configure(EntityTypeBuilder<FavoriteCourse> builder)
    {
        builder.ToTable("FavoriteCourses", SchemaConstants.Interactions);

        #region Keys

        // A user can favorite a course exactly once (s4.6)
        builder.HasKey(x => new { x.UserId, x.CourseId });

        #endregion

        #region Relations

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // The course must remain available while favorites reference it
        builder.HasOne(x => x.Course)
            .WithMany()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion
    }
}
