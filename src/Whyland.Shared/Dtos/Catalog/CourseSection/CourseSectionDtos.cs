namespace Whyland.Shared.Dtos.Catalog.CourseSection;

/// <summary>A section row in the course curriculum table.</summary>
public sealed class CourseSectionListItemDto
{
    /// <summary>The section identifier.</summary>
    public long Id { get; set; }

    /// <summary>The course the section belongs to.</summary>
    public long CourseId { get; set; }

    /// <summary>The section title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>The manual display position.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>How many live episodes the section currently has.</summary>
    public int EpisodeCount { get; set; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }
}
