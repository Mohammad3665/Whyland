using Whyland.Domain.Entities.Catalog;

namespace Whyland.Domain.Entities.Identity;

/// <summary>
/// Dedicated instructor information extending a <see cref="User"/> (s5.2).
/// Banking information is profile data only in this version; no payout workflow exists.
/// </summary>
public class InstructorProfile : BaseEntity<Guid>
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string? ProfileImage { get; set; }

    public string? Bio { get; set; }

    public string? Biography { get; set; }

    public string? Specialization { get; set; }

    public string? Website { get; set; }

    public string? LinkedIn { get; set; }

    public string? Instagram { get; set; }

    public string? NationalCode { get; set; }

    public string? Address { get; set; }

    public string? ShebaNumber { get; set; }

    public string? BankAccountNumber { get; set; }

    public ICollection<CourseInstructor> CourseInstructors { get; set; } = [];
}
