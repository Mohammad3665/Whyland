
using CourseEntity = Whyland.Domain.Model.Catalog.Course.Entities.Course;

namespace Whyland.Domain.Model.Catalog.CourseInstructor.Entities;

/// <summary>
/// Many-to-many relation between courses and instructor profiles (s7.5).
/// The instructor relation is maintained independently of authentication credentials;
/// only users designated as instructors are selectable (s7.5).
/// </summary>
public class CourseInstructor
{
    /// <summary>Parameterless constructor required by ORMs (e.g. EF Core).</summary>
    private CourseInstructor() { }

    /// <summary>
    /// Creates the link between a course and an instructor profile. The course navigation is set as
    /// well, so EF can fix up the foreign key while a new course still has no generated identifier.
    /// </summary>
    /// <param name="course">The course being linked.</param>
    /// <param name="instructorProfileId">The instructor profile to link.</param>
    public static CourseInstructor Create(CourseEntity course, InstructorProfileId instructorProfileId)
        => new() { Course = course, CourseId = course.Id, InstructorProfileId = instructorProfileId };

    /// <summary>
    /// The course this relation links to the instructor.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// The course this relation links to the instructor.
    /// </summary>
    public CourseEntity Course { get; private set; } = null!;

    /// <summary>
    /// The instructor profile this relation links to the course.
    /// </summary>
    public InstructorProfileId InstructorProfileId { get; private set; }

    /// <summary>
    /// The instructor profile this relation links to the course.
    /// </summary>
    public InstructorProfile InstructorProfile { get; private set; } = null!;
}
