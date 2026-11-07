namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Create;

/// <summary>Creates the instructor profile of an existing, active account.</summary>
public sealed record CreateInstructorProfileCommand(
    Guid UserId,
    string? Bio,
    string? Biography,
    string? Specialization,
    string? Website,
    string? LinkedIn,
    string? Instagram,
    string? ShebaNumber,
    string? BankAccountNumber,
    InstructorProfileImageUpload? Image) : ICommand<Guid>;
