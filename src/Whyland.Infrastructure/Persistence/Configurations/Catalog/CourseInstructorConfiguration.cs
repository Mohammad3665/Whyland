
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

/// <summary>
/// Maps the link between a course and its instructor profiles (s7.3).
/// </summary>
public sealed class CourseInstructorConfiguration : IEntityTypeConfiguration<CourseInstructor>
{
    public void Configure(EntityTypeBuilder<CourseInstructor> builder)
    {
        builder.ToTable("CourseInstructors", SchemaConstants.Catalog);

        #region Keys

        builder.HasKey(x => new { x.CourseId, x.InstructorProfileId });

        #endregion
    }
}
