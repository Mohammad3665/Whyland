namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the FAQ items of one course.
/// </summary>
/// <param name="CourseId">The course whose questions are reordered.</param>
/// <param name="OrderedIds">The question identifiers in the desired visual order; index + 1 becomes DisplayOrder.</param>
public sealed record ReorderCourseFaqsCommand(
    long CourseId,
    IReadOnlyList<long> OrderedIds) : ICommand;
