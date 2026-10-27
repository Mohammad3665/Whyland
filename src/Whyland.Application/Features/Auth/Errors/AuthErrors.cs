namespace Whyland.Application.Features.Auth.Errors;

/// <summary>
/// Central catalog of Error instances produced by the auth feature.
/// Auth failures that leak account existence use ErrorType.Unauthorized with a
/// single generic message (s15.1.3 — no user enumeration).
/// </summary>
public static class AuthErrors
{
    /// <summary>Invalid/expired/exhausted OTP — one generic message for all three cases.</summary>
    public static readonly Error InvalidOtp = new(
        "Auth.InvalidOtp",
        "کد واردشده نامعتبر یا منقضی است.",
        ErrorType.Unauthorized
    );

    /// <summary>User asked for a new code too soon (cooldown not elapsed).</summary>
    public static readonly Error OtpCooldown = new(
        "Auth.OtpCooldown",
        "کد جدید به‌تازگی ارسال شده است؛ لطفاً کمی بعد دوباره تلاش کنید.",
        ErrorType.Validation
    );

    /// <summary>Generic "could not complete login" for unexpected failures.</summary>
    public static readonly Error LoginFailed = new(
        "Auth.LoginFailed",
        "خطا در ورود؛ لطفاً مجدداً تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>
    /// The SMS service did not confirm delivery, so the code was invalidated before the user
    /// could see it. Retryable — resend is still governed by the cooldown.
    /// </summary>
    public static readonly Error SmsSendFailed = new(
        "Auth.SmsSendFailed",
        "ارسال پیامک ناموفق بود؛ لطفاً چند لحظه دیگر دوباره تلاش کنید.",
        ErrorType.Unexpected
    );

    /// <summary>Registration ticket unknown, already used, or expired — user must re-verify by code.</summary>
    public static readonly Error InvalidTicket = new(
        "Auth.InvalidTicket",
        "درخواست ثبت‌نام منقضی شده است؛ لطفاً دوباره کد تأیید بگیرید.",
        ErrorType.Unauthorized
    );

    /// <summary>The verified phone already belongs to an account (race); user should just sign in.</summary>
    public static readonly Error RegistrationConflict = new(
        "Auth.RegistrationConflict",
        "برای این شماره حسابی وجود دارد؛ لطفاً وارد شوید.",
        ErrorType.Conflict
    );

    /// <summary>
    /// Unknown account or wrong password — one generic message so the endpoint cannot be used
    /// to discover which identifiers have an account (s15.1.3).
    /// </summary>
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "نام کاربری/شماره/ایمیل یا رمز عبور نامعتبر است.",
        ErrorType.Unauthorized
    );

    /// <summary>Too many failed password attempts; the account is temporarily locked (s15.1.4).</summary>
    public static readonly Error AccountLockedOut = new(
        "Auth.AccountLockedOut",
        "به دلیل تلاش‌های ناموفق زیاد، این حساب موقتاً قفل شده است؛ کمی بعد دوباره تلاش کنید.",
        ErrorType.Unauthorized
    );

    /// <summary>The old password did not match the stored hash (only ever shown to the signed-in owner).</summary>
    public static readonly Error WrongPassword = new(
        "Auth.WrongPassword",
        "رمز عبور فعلی صحیح نیست.",
        ErrorType.Unauthorized
    );

    /// <summary>The account has no password yet, so it belongs to the set-password flow instead.</summary>
    public static readonly Error PasswordNotSet = new(
        "Auth.PasswordNotSet",
        "این حساب هنوز رمز عبوری ندارد؛ برای تعیین رمز از «تعیین رمز عبور» استفاده کنید.",
        ErrorType.Validation
    );

    /// <summary>The account already has a password, so it belongs to the change-password flow instead.</summary>
    public static readonly Error AlreadyHasPassword = new(
        "Auth.AlreadyHasPassword",
        "این حساب از قبل رمز عبور دارد؛ برای تغییر آن از «تغییر رمز عبور» استفاده کنید.",
        ErrorType.Validation
    );

    /// <summary>
    /// The phone number entered while setting a password already belongs to another account, so it
    /// cannot be bound twice (s5.4). Shown only to the signed-in owner of the account being edited.
    /// </summary>
    public static readonly Error PhoneTaken = new(
        "Auth.PhoneTaken",
        "این شماره موبایل برای حساب دیگری ثبت شده است.",
        ErrorType.Conflict
    );

    /// <summary>The account already has a verified phone, so no binding code is needed (s5.4).</summary>
    public static readonly Error PhoneAlreadySet = new(
        "Auth.PhoneAlreadySet",
        "حساب شما از قبل شماره موبایل تأییدشده دارد.",
        ErrorType.Validation
    );

    /// <summary>A password is being set on an account with no phone, so the number and its code are required.</summary>
    public static readonly Error PhoneRequired = new(
        "Auth.PhoneRequired",
        "برای تعیین رمز عبور، شماره موبایل و کد تأیید آن لازم است.",
        ErrorType.Validation
    );
}