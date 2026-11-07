namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetFilterFields;

/// <summary>Describes the fields the instructor table can be filtered and sorted on.</summary>
public sealed record GetInstructorProfileFilterFieldsQuery : IQuery<IReadOnlyList<FilterFieldDto>>;
