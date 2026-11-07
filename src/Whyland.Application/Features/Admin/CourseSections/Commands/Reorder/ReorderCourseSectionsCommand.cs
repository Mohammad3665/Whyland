namespace Whyland.Application.Features.Admin.CourseSections.Commands.Reorder;

/// <summary>Persists a new display order for the sections of one course.</summary>
public sealed record ReorderCourseSectionsCommand(
    long CourseId,
    IReadOnlyList<long> OrderedIds) : ICommand;
