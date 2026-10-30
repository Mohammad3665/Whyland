namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// Dedicated instructor information extending a <see cref="User"/> (s5.2).
/// Banking information is profile data only in this version; no payout workflow exists.
/// </summary>
public class InstructorProfile : BaseEntity<Guid>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="BankAccountNumber"/>; also the database column size.</summary>
    public const int MaxBankAccountNumberLength = 26;

    /// <summary>Maximum allowed length of <see cref="Bio"/>; also the database column size.</summary>
    public const int MaxBioLength = 1000;

    /// <summary>Maximum allowed length of <see cref="Instagram"/>; also the database column size.</summary>
    public const int MaxInstagramLength = 256;

    /// <summary>Maximum allowed length of <see cref="LinkedIn"/>; also the database column size.</summary>
    public const int MaxLinkedInLength = 256;

    /// <summary>Maximum allowed length of <see cref="ProfileImage"/>; also the database column size.</summary>
    public const int MaxProfileImageLength = 256;

    /// <summary>Maximum allowed length of <see cref="ShebaNumber"/>; also the database column size.</summary>
    public const int MaxShebaNumberLength = 26;

    /// <summary>Maximum allowed length of <see cref="Specialization"/>; also the database column size.</summary>
    public const int MaxSpecializationLength = 200;

    /// <summary>Maximum allowed length of <see cref="Website"/>; also the database column size.</summary>
    public const int MaxWebsiteLength = 256;

    #endregion
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
