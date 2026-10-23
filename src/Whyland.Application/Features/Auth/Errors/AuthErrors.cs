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
}