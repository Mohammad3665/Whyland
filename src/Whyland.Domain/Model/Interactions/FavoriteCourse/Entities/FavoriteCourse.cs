namespace Whyland.Domain.Model.Interactions.FavoriteCourse.Entities;

/// <summary>
/// The favorite ("wishlist") link a user creates for a course and sees on their profile (s4.6).
/// </summary>
public class FavoriteCourse
{
    #region Properties

    /// <summary>
    /// The user who favorited the course.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The favorited course.
    /// </summary>
    public CourseId CourseId { get; private set; }

    /// <summary>
    /// UTC time of the favorite action.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    #endregion

    #region Relations

    /// <summary>
    /// The user who favorited the course.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// The favorited course.
    /// </summary>
    public Course Course { get; private set; } = null!;

    #endregion
}
