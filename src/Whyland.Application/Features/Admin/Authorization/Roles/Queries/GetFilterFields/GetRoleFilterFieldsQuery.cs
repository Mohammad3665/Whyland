namespace Whyland.Application.Features.Admin.Authorization.Roles.Queries.GetFilterFields;

/// <summary>
/// Describes every role field the roles table can be filtered or sorted on, including the two counted
/// columns the listing computes, so the filter picker is built from the model instead of being
/// hand-written for this page.
/// </summary>
public sealed record GetRoleFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
