namespace Whyland.Shared.Dtos.Catalog.CourseFaq;

/// <summary>A question shown under the FAQ section of one course.</summary>
public sealed class CourseFaqListItemDto
{
    /// <summary>The FAQ identifier.</summary>
    public long Id { get; set; }

    /// <summary>The course the question belongs to.</summary>
    public long CourseId { get; set; }

    /// <summary>The question text.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The answer text.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The manual display position.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
