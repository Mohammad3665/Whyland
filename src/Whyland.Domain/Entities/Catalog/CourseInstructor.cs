namespace Whyland.Domain.Entities.Catalog;

/// <summary>
/// Many-to-many relation between courses and instructor profiles (s7.5).
/// The instructor relation is maintained independently of authentication credentials;
/// only users designated as instructors are selectable (s7.5).
/// </summary>
public class CourseInstructor
{
    /// <summary>
    /// The course this relation links to the instructor.
    /// </summary>
    public long CourseId { get; private set; }

    /// <summary>
    /// The course this relation links to the instructor.
    /// </summary>
    public Course Course { get; private set; } = null!;

    /// <summary>
    /// The instructor profile this relation links to the course.
    /// </summary>
    public Guid InstructorProfileId { get; private set; }

    /// <summary>
    /// The instructor profile this relation links to the course.
    /// </summary>
    public InstructorProfile InstructorProfile { get; private set; } = null!;
}
