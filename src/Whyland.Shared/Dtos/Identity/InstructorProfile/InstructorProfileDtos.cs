namespace Whyland.Shared.Dtos.Identity.InstructorProfile;

/// <summary>One row of the admin instructors table.</summary>
/// <param name="Id">Instructor profile identifier.</param>
/// <param name="UserId">Owning account identifier.</param>
/// <param name="FirstName">Account given name.</param>
/// <param name="LastName">Account family name.</param>
/// <param name="Email">Account e-mail, when present.</param>
/// <param name="PhoneNumber">Account phone number, when present.</param>
/// <param name="ProfileImageUrl">Public profile image URL, when set.</param>
/// <param name="Specialization">Instructor specialty, when set.</param>
/// <param name="CourseCount">Number of live courses assigned to the instructor.</param>
/// <param name="CreatedAt">UTC creation time.</param>
public sealed record InstructorProfileListItemDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    string? Specialization,
    int CourseCount,
    DateTime CreatedAt);

/// <summary>Everything the profile page and the edit form show for one instructor.</summary>
/// <param name="Id">Instructor profile identifier.</param>
/// <param name="UserId">Owning account identifier.</param>
/// <param name="FirstName">Account given name.</param>
/// <param name="LastName">Account family name.</param>
/// <param name="Email">Account e-mail, when present.</param>
/// <param name="PhoneNumber">Account phone number, when present.</param>
/// <param name="ProfileImageUrl">Public profile image URL, when set.</param>
/// <param name="Bio">Short biography.</param>
/// <param name="Biography">Full biography.</param>
/// <param name="Specialization">Area of expertise.</param>
/// <param name="Website">Personal website.</param>
/// <param name="LinkedIn">LinkedIn profile.</param>
/// <param name="Instagram">Instagram profile.</param>
/// <param name="ShebaNumber">Sheba number, without the IR prefix.</param>
/// <param name="BankAccountNumber">Bank account number.</param>
/// <param name="CourseCount">Number of live courses assigned to the instructor.</param>
/// <param name="CreatedAt">UTC creation time.</param>
/// <param name="UpdatedAt">UTC last-update time.</param>
public sealed record InstructorProfileDetailsDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    string? Bio,
    string? Biography,
    string? Specialization,
    string? Website,
    string? LinkedIn,
    string? Instagram,
    string? ShebaNumber,
    string? BankAccountNumber,
    int CourseCount,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>An active account that can still be given an instructor profile.</summary>
/// <param name="Id">Account identifier.</param>
/// <param name="Name">Full name shown in the selector.</param>
/// <param name="Email">Account e-mail, when present.</param>
/// <param name="PhoneNumber">Account phone number, when present.</param>
public sealed record InstructorUserOptionDto(
    Guid Id,
    string Name,
    string? Email,
    string? PhoneNumber);

/// <summary>The account list the create form chooses from.</summary>
/// <param name="Users">Active accounts without an instructor profile.</param>
public sealed record InstructorProfileFormOptionsDto(
    IReadOnlyList<InstructorUserOptionDto> Users);
