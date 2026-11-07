using Whyland.Application.Common.Abstractions.Storage;

namespace Whyland.Application.Features.Admin.Courses;

/// <summary>
/// One uploaded cover image handed to the application layer. The page model opens the form-file
/// stream and disposes it after the dispatch returns; the command and its handlers never see
/// ASP.NET types.
/// </summary>
/// <param name="Content">The uploaded bytes.</param>
/// <param name="FileName">Original file name, used to check the extension.</param>
/// <param name="ContentType">Declared content type, when the transport provides one.</param>
/// <param name="Length">Declared length, re-checked against the bytes actually read.</param>
public sealed record CourseImageUpload(
    Stream Content,
    string FileName,
    string? ContentType,
    long Length);
