namespace Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the sections of one course (s7.6).
/// </summary>
/// <param name="CourseId">The course whose sections are reordered.</param>
/// <param name="OrderedIds">The section identifiers in the desired visual order; index + 1 becomes DisplayOrder.</param>
public sealed record ReorderCourseSectionsCommand(
    long CourseId,
    IReadOnlyList<long> OrderedIds) : ICommand;
