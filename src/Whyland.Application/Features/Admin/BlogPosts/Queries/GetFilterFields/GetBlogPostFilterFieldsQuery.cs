namespace Whyland.Application.Features.Admin.BlogPosts.Queries.GetFilterFields;

/// <summary>
/// Describes every blog post field the table can be filtered or sorted on, so the filter picker is
/// built from the model instead of being hand-written per field.
/// </summary>
public sealed record GetBlogPostFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
