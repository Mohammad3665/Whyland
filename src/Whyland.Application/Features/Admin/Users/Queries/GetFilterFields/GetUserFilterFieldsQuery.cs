namespace Whyland.Application.Features.Admin.Users.Queries.GetFilterFields;

/// <summary>
/// Describes every account field the users table can be filtered or sorted on, so the
/// filter picker is built from the model instead of being hand-written per field.
/// </summary>
public sealed record GetUserFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
