using Whyland.Domain.Common.Errors;

namespace Whyland.Application.Features.Admin.Users.Errors;

/// <summary>
/// Errors produced by the admin users catalog. Uniqueness of the sign-in identifier
/// (phone number) and of the e-mail is reported as a validation failure so the modal can
/// show it; persistence problems stay behind a generic message.
/// </summary>
public static class UserAdminErrors
{
    /// <summary>No account was found with the given identifier.</summary>
    public static Error NotFound(Guid id) => new(
        "User.NotFound",
        $"کاربری با شناسه {id} یافت نشد.",
        ErrorType.NotFound
    );

    /// <summary>The phone number already belongs to another account.</summary>
    public static Error PhoneTaken(string phoneNumber) => new(
        "User.PhoneTaken",
        $"شماره موبایل {phoneNumber} برای کاربر دیگری ثبت شده است.",
        ErrorType.Validation
    );

    /// <summary>The e-mail already belongs to another account.</summary>
    public static Error EmailTaken(string email) => new(
        "User.EmailTaken",
        $"ایمیل {email} برای کاربر دیگری ثبت شده است.",
        ErrorType.Validation
    );

    /// <summary>None of the submitted role identifiers exists.</summary>
    public static Error RolesNotFound() => new(
        "User.RolesNotFound",
        "نقش انتخاب‌شده معتبر نیست.",
        ErrorType.Validation
    );

    /// <summary>The account could not be created (typically a persistence failure).</summary>
    public static Error CreateFailed(string fullName) => new(
        "User.CreateFailed",
        $"خطا در ایجاد کاربر {fullName}، لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>The account could not be updated (typically a persistence failure).</summary>
    public static Error UpdateFailed(Guid id) => new(
        "User.UpdateFailed",
        $"خطا در ویرایش کاربر با شناسه {id}، لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>The seeded super admin must stay active, otherwise the panel locks everyone out.</summary>
    public static Error SuperAdminProtected() => new(
        "User.SuperAdminProtected",
        "حساب مدیر کل سیستم غیرفعال نمی‌شود.",
        ErrorType.Business
    );

    /// <summary>The users list could not be read (typically a persistence failure).</summary>
    public static Error ListFailed() => new(
        "User.ListFailed",
        "خطا در دریافت لیست کاربران، لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>The assignable roles could not be read (typically a persistence failure).</summary>
    public static Error RolesListFailed() => new(
        "User.RolesListFailed",
        "خطا در دریافت لیست نقش‌ها، لطفا مجددا تلاش کنید.",
        ErrorType.Unexpected
    );
}
