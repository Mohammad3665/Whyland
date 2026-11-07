namespace Whyland.Application.Features.Admin.InstructorProfiles.Queries.GetById;

/// <summary>Returns the full instructor profile shown in the page and edit form.</summary>
public sealed record GetInstructorProfileByIdQuery(Guid Id) : IQuery<InstructorProfileDetailsDto>;
