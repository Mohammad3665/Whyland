using Whyland.Domain.Common.Errors;
using Whyland.Domain.Model.Identity.InstructorProfile.Errors;
using Whyland.Shared.Results;
using UserEntity = Whyland.Domain.Model.Identity.User.Entities.User;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Entities;

/// <summary>
/// Dedicated instructor information extending a <see cref="UserEntity"/> (s5.2).
/// Banking information is profile data only in this version; no payout workflow exists.
/// </summary>
public class InstructorProfile : BaseEntity<InstructorProfileId>
{
    #region Limits

    // Every column length is declared once on the entity that owns the column, so the
    // EF mapping, the request validators and the domain rules all read the same number.
    // Changing a size means editing one line here.

    /// <summary>Maximum allowed length of <see cref="BankAccountNumber"/>; also the database column size.</summary>
    public const int MaxBankAccountNumberLength = 26;

    /// <summary>Maximum allowed length of <see cref="Bio"/>; also the database column size.</summary>
    public const int MaxBioLength = 1000;

    /// <summary>Maximum allowed length of <see cref="Biography"/>; also the database column size.</summary>
    public const int MaxBiographyLength = 4000;

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

    #region Identity

    /// <summary>
    /// The user the instructor information belongs to; shares the user's identifier.
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// The user the instructor information belongs to.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    #endregion

    #region Profile

    /// <summary>
    /// Path or URL of the profile image shown in instructor listings.
    /// </summary>
    public string? ProfileImage { get; private set; }

    /// <summary>
    /// Short bio used in listings and public instructor summaries.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// Full biography with extended details shown on the instructor page.
    /// </summary>
    public string? Biography { get; private set; }

    /// <summary>
    /// The instructor's area of expertise.
    /// </summary>
    public string? Specialization { get; private set; }

    /// <summary>
    /// Personal website link, if provided.
    /// </summary>
    public string? Website { get; private set; }

    /// <summary>
    /// LinkedIn profile link, if provided.
    /// </summary>
    public string? LinkedIn { get; private set; }

    /// <summary>
    /// Instagram profile link, if provided.
    /// </summary>
    public string? Instagram { get; private set; }

    /// <summary>
    /// Sheba (IBAN) number for banking information (s5.2). Profile data only in this version.
    /// </summary>
    public string? ShebaNumber { get; private set; }

    /// <summary>
    /// Bank account number for banking information (s5.2). Profile data only in this version.
    /// </summary>
    public string? BankAccountNumber { get; private set; }

    #endregion

    /// <summary>
    /// The courses this instructor is assigned to teach.
    /// </summary>
    public ICollection<CourseInstructor> CourseInstructors { get; } = [];

    #region Construction

    /// <summary>Parameterless constructor required by ORMs (e.g. EF Core).</summary>
    private InstructorProfile() { }

    /// <summary>
    /// Creates the teaching profile of an existing account. The caller is responsible for proving
    /// that the account exists, is active and does not already have a profile.
    /// </summary>
    public static Result<InstructorProfile> Create(
        UserId userId,
        string? bio = null,
        string? biography = null,
        string? specialization = null,
        string? website = null,
        string? linkedIn = null,
        string? instagram = null,
        string? shebaNumber = null,
        string? bankAccountNumber = null)
    {
        if (userId.Value == Guid.Empty)
            return InstructorProfileErrors.FieldRequired(nameof(UserId));

        var profile = new InstructorProfile
        {
            Id = InstructorProfileId.From(Guid.NewGuid()),
            UserId = userId,
        };

        var applied = profile.Apply(
            bio,
            biography,
            specialization,
            website,
            linkedIn,
            instagram,
            shebaNumber,
            bankAccountNumber);

        return applied.IsFailure ? applied.Error : profile;
    }

    #endregion

    #region Behaviour

    /// <summary>
    /// Rewrites the editable profile fields. The owning account is immutable once the profile exists.
    /// </summary>
    public Result Update(
        string? bio,
        string? biography,
        string? specialization,
        string? website,
        string? linkedIn,
        string? instagram,
        string? shebaNumber,
        string? bankAccountNumber)
        => Apply(
            bio,
            biography,
            specialization,
            website,
            linkedIn,
            instagram,
            shebaNumber,
            bankAccountNumber);

    /// <summary>
    /// Stores the relative path of the current profile image, or clears it. The image pipeline
    /// owns the file; the aggregate only keeps the path it was given.
    /// </summary>
    public Result SetProfileImage(string? profileImage)
    {
        var cleaned = Clean(profileImage);
        if (cleaned?.Length > MaxProfileImageLength)
            return InstructorProfileErrors.FieldTooLong(nameof(ProfileImage), MaxProfileImageLength);

        ProfileImage = cleaned;
        return Result.Success();
    }

    /// <summary>
    /// Cleans, validates and assigns every editable profile value. Shared by create and update so
    /// both paths enforce exactly the same rules; on failure the instance is left unchanged.
    /// </summary>
    /// <returns>A successful <see cref="Result"/> if all rules pass; otherwise a validation failure.</returns>
    private Result Apply(
        string? bio,
        string? biography,
        string? specialization,
        string? website,
        string? linkedIn,
        string? instagram,
        string? shebaNumber,
        string? bankAccountNumber)
    {
        var lengthError =
            LengthError(nameof(Bio), bio, MaxBioLength) ??
            LengthError(nameof(Biography), biography, MaxBiographyLength) ??
            LengthError(nameof(Specialization), specialization, MaxSpecializationLength) ??
            LengthError(nameof(Website), website, MaxWebsiteLength) ??
            LengthError(nameof(LinkedIn), linkedIn, MaxLinkedInLength) ??
            LengthError(nameof(Instagram), instagram, MaxInstagramLength) ??
            LengthError(nameof(ShebaNumber), shebaNumber, MaxShebaNumberLength) ??
            LengthError(nameof(BankAccountNumber), bankAccountNumber, MaxBankAccountNumberLength);

        if (lengthError is not null)
            return lengthError;

        var shebaValue = Clean(shebaNumber);
        if (shebaValue is not null && (shebaValue.Length != 24 || !shebaValue.All(char.IsDigit)))
            return InstructorProfileErrors.InvalidShebaNumber();

        var accountValue = Clean(bankAccountNumber);
        if (accountValue is not null &&
            (accountValue.Length is < 6 or > 20 || !accountValue.All(char.IsDigit)))
            return InstructorProfileErrors.InvalidBankAccountNumber();

        Bio = Clean(bio);
        Biography = Clean(biography);
        Specialization = Clean(specialization);
        Website = Clean(website);
        LinkedIn = Clean(linkedIn);
        Instagram = Clean(instagram);
        ShebaNumber = Clean(shebaNumber);
        BankAccountNumber = Clean(bankAccountNumber);

        return Result.Success();
    }

    private static DomainError? LengthError(string field, string? value, int max)
    {
        var cleaned = Clean(value);
        return cleaned?.Length > max ? InstructorProfileErrors.FieldTooLong(field, max) : null;
    }

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    #endregion
}
