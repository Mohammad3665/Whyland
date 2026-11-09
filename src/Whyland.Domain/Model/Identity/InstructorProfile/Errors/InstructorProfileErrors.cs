using Whyland.Domain.Common.Errors;

using InstructorProfileEntity = Whyland.Domain.Model.Identity.InstructorProfile.Entities.InstructorProfile;

namespace Whyland.Domain.Model.Identity.InstructorProfile.Errors;

/// <summary>
/// Every failure the <see cref="InstructorProfileEntity"/> aggregate and its handlers can report.
/// The wording lives in the localization catalog.
/// </summary>
public static class InstructorProfileErrors
{
    /// <summary>The specified field is required but was empty or whitespace.</summary>
    public static DomainError FieldRequired(string field) =>
        DomainError.For<InstructorProfileEntity>(nameof(FieldRequired), ErrorType.Validation, field: field);

    /// <summary>The specified field exceeds its maximum allowed length.</summary>
    public static DomainError FieldTooLong(string field, int max) =>
        DomainError.For<InstructorProfileEntity>(nameof(FieldTooLong), ErrorType.Validation, field: field, ("Max", max));

    /// <summary>The sheba number is not exactly twenty-four digits.</summary>
    public static DomainError InvalidShebaNumber() =>
        DomainError.For<InstructorProfileEntity>(nameof(InvalidShebaNumber), ErrorType.Validation, field: "ShebaNumber");

    /// <summary>The bank account number is not the expected 6–20 digits.</summary>
    public static DomainError InvalidBankAccountNumber() =>
        DomainError.For<InstructorProfileEntity>(nameof(InvalidBankAccountNumber), ErrorType.Validation, field: "BankAccountNumber");

    /// <summary>A social link is not an absolute http(s) address.</summary>
    public static DomainError InvalidSocialUrl(string field) =>
        DomainError.For<InstructorProfileEntity>(nameof(InvalidSocialUrl), ErrorType.Validation, field: field);

    /// <summary>No instructor profile exists with the given identifier.</summary>
    public static DomainError NotFound(InstructorProfileId id) =>
        DomainError.For<InstructorProfileEntity>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>The account already has an instructor profile.</summary>
    public static DomainError AlreadyExists(UserId userId) =>
        DomainError.For<InstructorProfileEntity>(nameof(AlreadyExists), ErrorType.Conflict, field: null, ("UserId", userId));

    /// <summary>The account the profile belongs to does not exist.</summary>
    public static DomainError UserNotFound(UserId userId) =>
        DomainError.For<InstructorProfileEntity>(nameof(UserNotFound), ErrorType.NotFound, field: "UserId", ("UserId", userId));

    /// <summary>The account exists but is disabled and cannot be made an instructor.</summary>
    public static DomainError UserInactive(UserId userId) =>
        DomainError.For<InstructorProfileEntity>(nameof(UserInactive), ErrorType.Validation, field: "UserId", ("UserId", userId));

    /// <summary>The profile could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<InstructorProfileEntity>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The profile could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<InstructorProfileEntity>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The profiles could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<InstructorProfileEntity>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The requested profile could not be read (persistence failure).</summary>
    public static DomainError DetailsFailed() =>
        DomainError.For<InstructorProfileEntity>(nameof(DetailsFailed), ErrorType.Unexpected);

    /// <summary>The account options for the create form could not be read (persistence failure).</summary>
    public static DomainError OptionsFailed() =>
        DomainError.For<InstructorProfileEntity>(nameof(OptionsFailed), ErrorType.Unexpected);
}
