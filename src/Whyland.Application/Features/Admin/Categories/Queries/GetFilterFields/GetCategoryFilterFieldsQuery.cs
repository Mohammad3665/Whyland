namespace Whyland.Application.Features.Admin.Categories.Queries.GetFilterFields;

/// <summary>
/// Describes every category field the categories table can be filtered or sorted on, so the
/// filter picker is built from the model instead of being hand-written per field.
/// </summary>
public sealed record GetCategoryFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
