
namespace Whyland.Infrastructure.Persistence.Configurations.Catalog;

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
