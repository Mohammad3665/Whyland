namespace Whyland.Application.Features.Admin.BlogPosts;

/// <summary>
/// The optional cover image uploaded with the blog post form. The stream's lifetime belongs to the
/// request, so the command carries it only for the duration of the handler.
/// </summary>
/// <param name="Content">The uploaded bytes.</param>
/// <param name="FileName">Original file name, used to check the extension.</param>
/// <param name="ContentType">Declared content type, when the transport provides one.</param>
/// <param name="Length">Declared length, re-checked against the bytes actually read.</param>
public sealed record BlogPostImageUpload(
    Stream Content,
    string FileName,
    string? ContentType,
    long Length);
