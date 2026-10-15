
namespace Whyland.Domain.Entities.Content;

/// <summary>
/// A standalone FAQ item shown in the general-purpose FAQ section (s4.12).
/// </summary>
public class Faq : BaseEntity<long>
{
    #region Properties

    /// <summary>
    /// The question text.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The answer text.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Manual ordering weight (s4.12).
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Inactive FAQs are hidden from the site but kept for editing.
    /// </summary>
    public bool IsActive { get; set; }

    #endregion
}
