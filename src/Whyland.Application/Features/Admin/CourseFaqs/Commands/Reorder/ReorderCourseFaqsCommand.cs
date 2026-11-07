namespace Whyland.Application.Features.Admin.CourseFaqs.Commands.Reorder;

/// <summary>Persists a new display order for the FAQs of one course.</summary>
public sealed record ReorderCourseFaqsCommand(
    long CourseId,
    IReadOnlyList<long> OrderedIds) : ICommand;
