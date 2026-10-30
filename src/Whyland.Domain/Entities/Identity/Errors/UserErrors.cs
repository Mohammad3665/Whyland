using Whyland.Domain.Common.Errors;

namespace Whyland.Domain.Entities.Identity.Errors;

/// <summary>
/// Every failure the <see cref="User"/> aggregate can report, whether it is raised while signing in,
/// while self-serving a password, or while an administrator edits the account.
/// <para>
/// One catalog per aggregate is deliberate: the code is <c>User.{name}</c>, so the same fact raised by
/// two different flows shares one key and one message. Each factory carries only the code, the category
/// and the arguments the message needs — the wording itself lives in the localization catalog.
/// </para>
/// </summary>
public static class UserErrors
{
    #region Sign-in and registration

    /// <summary>
    /// The submitted one-time code is unknown, wrong, expired or already exhausted. One code for every
    /// case so the response cannot tell an attacker which of the four happened.
    /// </summary>
    public static DomainError InvalidOtp() =>
        DomainError.For<User>(nameof(InvalidOtp), ErrorType.Unauthorized);

    /// <summary>A new code was requested before the cooldown of the previous one elapsed.</summary>
    public static DomainError OtpCooldown() =>
        DomainError.For<User>(nameof(OtpCooldown), ErrorType.Validation);

    /// <summary>Sign-in could not complete for a reason the user cannot act on.</summary>
    public static DomainError LoginFailed() =>
        DomainError.For<User>(nameof(LoginFailed), ErrorType.Unexpected);

    /// <summary>The registration ticket is unknown, already used or expired, so the phone must be verified again.</summary>
    public static DomainError InvalidTicket() =>
        DomainError.For<User>(nameof(InvalidTicket), ErrorType.Unauthorized);

    /// <summary>The verified phone already belongs to an account, so registration ends and sign-in begins.</summary>
    public static DomainError RegistrationConflict() =>
        DomainError.For<User>(nameof(RegistrationConflict), ErrorType.Conflict);

    /// <summary>
    /// Unknown account or wrong password. Both cases answer with this single error so the endpoint cannot
    /// be used to discover which identifiers have an account.
    /// </summary>
    public static DomainError InvalidCredentials() =>
        DomainError.For<User>(nameof(InvalidCredentials), ErrorType.Unauthorized);

    /// <summary>Too many failed password attempts; the account is locked until the lockout window passes.</summary>
    public static DomainError AccountLockedOut() =>
        DomainError.For<User>(nameof(AccountLockedOut), ErrorType.Unauthorized);

    #endregion

    #region  Passwords

    /// <summary>The current password did not match. Only ever shown to the signed-in owner of the account.</summary>
    public static DomainError WrongPassword() =>
        DomainError.For<User>(nameof(WrongPassword), ErrorType.Unauthorized);

    /// <summary>The account has no password yet, so this belongs to the set-password flow instead.</summary>
    public static DomainError PasswordNotSet() =>
        DomainError.For<User>(nameof(PasswordNotSet), ErrorType.Validation);

    /// <summary>The account already has a password, so this belongs to the change-password flow instead.</summary>
    public static DomainError AlreadyHasPassword() =>
        DomainError.For<User>(nameof(AlreadyHasPassword), ErrorType.Validation);

    /// <summary>The chosen password is too weak: it needs at least one letter and one digit.</summary>
    public static DomainError PasswordTooWeak() =>
        DomainError.For<User>(nameof(PasswordTooWeak), ErrorType.Validation);

    #endregion

    #region  Phone binding

    /// <summary>
    /// The phone number is already attached to another account, so it cannot be bound twice. Raised both
    /// when the owner binds a number while setting a password and when an administrator submits it.
    /// </summary>
    /// <param name="phoneNumber">The number that is taken; echoed back so the user sees which input clashed.</param>
    public static DomainError PhoneTaken(string phoneNumber) =>
        DomainError.For<User>(nameof(PhoneTaken), ErrorType.Conflict, field: nameof(User.PhoneNumber), ("PhoneNumber", phoneNumber));

    /// <summary>The account already has a verified number, so no binding code is needed.</summary>
    public static DomainError PhoneAlreadySet() =>
        DomainError.For<User>(nameof(PhoneAlreadySet), ErrorType.Validation);

    /// <summary>A password is being set on an account with no number, so the number and its code are required.</summary>
    public static DomainError PhoneRequired() =>
        DomainError.For<User>(nameof(PhoneRequired), ErrorType.Validation);

    #endregion

    #region  Administration 

    /// <summary>No account exists with the given identifier.</summary>
    /// <param name="id">The identifier that was looked up.</param>
    public static DomainError NotFound(Guid id) =>
        DomainError.For<User>(nameof(NotFound), ErrorType.NotFound, field: null, ("Id", id));

    /// <summary>The e-mail is already attached to another account.</summary>
    /// <param name="email">The address that is taken.</param>
    public static DomainError EmailTaken(string email) =>
        DomainError.For<User>(nameof(EmailTaken), ErrorType.Conflict, field: nameof(User.Email), ("Email", email));

    /// <summary>None of the submitted role identifiers exists, so the assignment cannot be saved.</summary>
    public static DomainError RolesNotFound() =>
        DomainError.For<User>(nameof(RolesNotFound), ErrorType.Validation, field: nameof(User.UserRoles));

    /// <summary>The seeded super admin stays active; deactivating it would lock everyone out of the panel.</summary>
    public static DomainError SuperAdminProtected() =>
        DomainError.For<User>(nameof(SuperAdminProtected), ErrorType.Business);

    /// <summary>The account could not be created (persistence failure); masked behind a generic message.</summary>
    public static DomainError CreateFailed() =>
        DomainError.For<User>(nameof(CreateFailed), ErrorType.Unexpected);

    /// <summary>The account could not be saved (persistence failure); masked behind a generic message.</summary>
    public static DomainError UpdateFailed() =>
        DomainError.For<User>(nameof(UpdateFailed), ErrorType.Unexpected);

    /// <summary>The accounts could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError ListFailed() =>
        DomainError.For<User>(nameof(ListFailed), ErrorType.Unexpected);

    /// <summary>The assignable roles could not be read (persistence failure); masked behind a generic message.</summary>
    public static DomainError RolesListFailed() =>
        DomainError.For<User>(nameof(RolesListFailed), ErrorType.Unexpected);

    #endregion
}
