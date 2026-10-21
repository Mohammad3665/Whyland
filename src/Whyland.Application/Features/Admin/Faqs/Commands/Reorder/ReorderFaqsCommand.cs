namespace Whyland.Application.Features.Admin.Faqs.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the FAQ aggregate (s4.12).
/// </summary>
/// <param name="OrderedIds">The FAQ identifiers in the desired visual order; index + 1 becomes DisplayOrder.</param>
public sealed record ReorderFaqsCommand(IReadOnlyList<long> OrderedIds) : ICommand;
