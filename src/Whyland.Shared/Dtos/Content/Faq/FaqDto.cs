namespace Whyland.Shared.Dtos.Content.Faq;

/// <summary>
/// Read model representing a single FAQ item as returned to admin clients.
/// </summary>
/// <param name="Id">The unique identifier of the FAQ.</param>
/// <param name="Title">The question text.</param>
/// <param name="Description">The answer text.</param>
/// <param name="DisplayOrder">
/// Manual ordering weight; lower values appear first.
/// </param>
/// <param name="IsActive">
/// Whether the FAQ is currently visible on the site.
/// Inactive items are hidden but kept for editing.
/// </param>
/// <param name="CreatedAt">The UTC timestamp when the FAQ was created.</param>
/// <param name="UpdatedAt">The UTC timestamp of the last update, if any.</param>
public sealed record FaqDto(
    long Id,
    string Title,
    string Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);