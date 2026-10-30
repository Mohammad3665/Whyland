namespace Whyland.Domain.Entities.Interactions;

/// <summary>
/// The favorite ("wishlist") link a user creates for a course and sees on their profile (s4.6).
/// </summary>
public class FavoriteCourse
{
    #region Properties

    /// <summary>
    /// The user who favorited the course.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The favorited course.
    /// </summary>
    public long CourseId { get; set; }

    /// <summary>
    /// UTC time of the favorite action.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    #endregion

    #region Relations

    /// <summary>
    /// The user who favorited the course.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The favorited course.
    /// </summary>
    public Course Course { get; set; } = null!;

    #endregion
}
