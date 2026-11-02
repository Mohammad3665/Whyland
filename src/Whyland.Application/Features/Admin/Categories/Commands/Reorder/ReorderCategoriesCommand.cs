namespace Whyland.Application.Features.Admin.Categories.Commands.Reorder;

/// <summary>
/// Command that persists a new DisplayOrder sequence for the category hierarchy after a
/// drag &amp; drop (s7.1).
/// </summary>
/// <param name="OrderedIds">
/// The visible category identifiers in their new order; each row is renumbered inside its own
/// sibling group, so one branch can move without scrambling the others.
/// </param>
public sealed record ReorderCategoriesCommand(IReadOnlyList<long> OrderedIds) : ICommand;
