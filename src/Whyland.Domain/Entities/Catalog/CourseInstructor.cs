using Whyland.Domain.Entities.Identity;

namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// Many-to-many relation between courses and instructor profiles (s7.5).
/// The instructor relation is maintained independently of authentication credentials;
/// only users designated as instructors are selectable (s7.5).
/// </summary>
public class CourseInstructor
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public Guid InstructorProfileId { get; set; }

    public InstructorProfile InstructorProfile { get; set; } = null!;
}
