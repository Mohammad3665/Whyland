namespace Whyland.Application.Features.Admin.InstructorProfiles;

/// <summary>
/// One uploaded instructor profile image handed to the application layer. The page model opens
/// the form-file stream and disposes it after the dispatch returns.
/// </summary>
public sealed record InstructorProfileImageUpload(
    Stream Content,
    string FileName,
    string? ContentType,
    long Length);
