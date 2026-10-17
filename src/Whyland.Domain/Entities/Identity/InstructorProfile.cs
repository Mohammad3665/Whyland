namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// Dedicated instructor information extending a <see cref="User"/> (s5.2).
/// Banking information is profile data only in this version; no payout workflow exists.
/// </summary>
public class InstructorProfile : BaseEntity<Guid>
{
    /// <summary>
    /// The user the instructor information belongs to; shares the user's identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user the instructor information belongs to.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Path or URL of the profile image shown in instructor listings.
    /// </summary>
    public string? ProfileImage { get; set; }

    /// <summary>
    /// Short bio used in listings and public instructor summaries.
    /// </summary>
    public string? Bio { get; set; }

    /// <summary>
    /// Full biography with extended details shown on the instructor page.
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// The instructor's area of expertise.
    /// </summary>
    public string? Specialization { get; set; }

    /// <summary>
    /// Personal website link, if provided.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// LinkedIn profile link, if provided.
    /// </summary>
    public string? LinkedIn { get; set; }

    /// <summary>
    /// Instagram profile link, if provided.
    /// </summary>
    public string? Instagram { get; set; }

    /// <summary>
    /// Sheba (IBAN) number for banking information (s5.2). Profile data only in this version.
    /// </summary>
    public string? ShebaNumber { get; set; }

    /// <summary>
    /// Bank account number for banking information (s5.2). Profile data only in this version.
    /// </summary>
    public string? BankAccountNumber { get; set; }

    /// <summary>
    /// The courses this instructor is assigned to teach.
    /// </summary>
    public ICollection<CourseInstructor> CourseInstructors { get; set; } = [];
}
