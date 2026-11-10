namespace Whyland.Application.Features.Admin.BlogCategories.Queries.GetFilterFields;

/// <summary>
/// Describes every blog category field the table can be filtered or sorted on, so the filter picker
/// is built from the model instead of being hand-written per field.
/// </summary>
public sealed record GetBlogCategoryFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;