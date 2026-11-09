namespace Whyland.Application.Features.Admin.InstructorProfiles.Commands.Update;

/// <summary>Updates an existing instructor profile and optionally replaces or removes its image.</summary>
public sealed record UpdateInstructorProfileCommand(
    InstructorProfileId Id,
    string? Bio,
    string? Biography,
    string? Specialization,
    string? Website,
    string? LinkedIn,
    string? Instagram,
    string? ShebaNumber,
    string? BankAccountNumber,
    bool RemoveImage,
    InstructorProfileImageUpload? Image) : ICommand;
