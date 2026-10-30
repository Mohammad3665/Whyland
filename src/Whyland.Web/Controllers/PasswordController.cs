using Whyland.Application.Features.Auth.Password.ChangePassword;
using Whyland.Application.Features.Auth.Password.ForgotPassword;
using Whyland.Application.Features.Auth.Password.RequestPhoneBindingCode;
using Whyland.Application.Features.Auth.Password.SetPassword;
using Whyland.Application.Features.Auth.Queries.PasswordState;
using Whyland.Web.ViewModels;

namespace Whyland.Web.Controllers;

/// <summary>
/// The password flows (s5.4, s13.2.3) as three separate responsibilities:
/// <list type="bullet">
///   <item><c>Forgot</c> — anonymous recovery for someone who cannot sign in: a six-digit code is
///   texted to the account's phone number and the new password is stored on the same page once the
///   code is proved. No link, no token.</item>
///   <item><c>SetPassword</c> — a signed-in account stores its first password, binding a verified
///   phone number first when it has none; that number is where a later recovery code is sent.</item>
///   <item><c>Change</c> — a signed-in account replaces the password it already has, proving the old
///   one. Nothing else happens on that page.</item>
/// </list>
/// <para>
/// Recovery and password login resolve accounts by phone number only — an email address is never an
/// identifier (business policy, s5.1.1).
/// </para>
/// <para>
/// The two signed-in pages redirect to one another according to the account state, so landing on the
/// wrong one is never an error the user has to interpret. Validation problems come back as failed
/// <see cref="Result"/>s (see ValidationBehavior), so no action here needs its own model-state checks,
/// and messages reach the page as hidden <c>data-toast</c> elements shown by the toast component.
/// </para>
/// </summary>
/// <param name="dispatcher">Sends the password commands and queries to the application layer.</param>
/// <param name="currentUser">Supplies the signed-in user id for the two signed-in flows.</param>
public class PasswordController(IDispatcher dispatcher, ICurrentUserService currentUser) : Controller
{
    /// <summary>Renders the "forgot password" page: the number first, then its code and the new password.</summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Forgot() =>
        View(new ForgotPasswordViewModel { PhoneNumber = TempData["ForgotPhone"] as string ?? string.Empty });

    /// <summary>
    /// Texts the recovery code to the number. The answer is identical for an existing and an unknown
    /// account, so this page cannot be used to enumerate users.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Forgot(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            return ForgotWithError("شماره موبایل را وارد کنید.", model.PhoneNumber);

        var result = await dispatcher.Send(new ForgotPasswordCommand(model.PhoneNumber), ct);

        if (result.IsFailure)
            return ForgotWithError(result.Error.Description, model.PhoneNumber);

        // The same wording whether or not an account matched: this page must not confirm existence.
        // The number rides in TempData so the code step is rendered with it already filled in.
        TempData["ForgotPhone"] = model.PhoneNumber;
        TempData["PasswordMessage"] =
            "در صورت وجود حسابی با این شماره، کد بازنشانی رمز عبور برایتان پیامک شد.";

        return RedirectToAction(nameof(Forgot));
    }

    /// <summary>Proves the recovery code and stores the new password in the same request.</summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotConfirm(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (model.NewPassword != model.ConfirmPassword)
            return ForgotWithError("رمز عبور و تایید آن یکسان نیستند.", model.PhoneNumber, model.Code);

        var result = await dispatcher.Send(
            new ForgotPasswordConfirmCommand(model.PhoneNumber, Digits.ToLatin(model.Code), model.NewPassword ?? string.Empty), ct);

        if (result.IsFailure)
            return ForgotWithError(result.Error.Description, model.PhoneNumber, model.Code);

        TempData["PasswordMessage"] = "رمز عبور با موفقیت بازنشانی شد؛ اکنون می‌توانید وارد شوید.";
        return RedirectToAction(nameof(AccountController.Login), "Account");
    }

    /// <summary>Renders the first-password form for the signed-in account.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SetPassword(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return RedirectToAction(nameof(AccountController.Login), "Account");

        var state = await ReadStateAsync(userId.Value, ct);
        if (state.HasPassword)
            return RedirectToAction(nameof(Change));

        return View(new SetPasswordViewModel { RequiresPhoneNumber = state.RequiresPhoneNumber });
    }

    /// <summary>
    /// Stores the first password of the signed-in account, binding the phone number it is missing
    /// once that number has proved itself with an SMS code (s5.4).
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model, CancellationToken ct)
    {
        // From the cookie, so one session can never target another account.
        var userId = currentUser.UserId;
        if (userId is null)
            return RedirectToAction(nameof(AccountController.Login), "Account");

        if (model.NewPassword != model.ConfirmPassword)
            return await SetPasswordWithErrorAsync("رمز عبور و تایید آن یکسان نیستند.", userId.Value, model, ct);

        var result = await dispatcher.Send(
            new SetPasswordCommand(userId.Value, model.PhoneNumber, Digits.ToLatin(model.Code), model.NewPassword), ct);

        if (result.IsFailure)
            return await SetPasswordWithErrorAsync(result.Error.Description, userId.Value, model, ct);

        // Done with this page: the account now has a password, so staying on (or returning to)
        // SetPassword would immediately redirect to the change page and the user would land on a
        // form they never asked for.
        TempData["PasswordMessage"] = "رمز عبور شما تنظیم شد؛ اکنون می‌توانید با رمز عبور وارد شوید.";
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Renders the change form for a signed-in account that already has a password.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Change(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return RedirectToAction(nameof(AccountController.Login), "Account");

        var state = await ReadStateAsync(userId.Value, ct);
        if (!state.HasPassword)
            return RedirectToAction(nameof(SetPassword));

        return View(new ChangePasswordViewModel());
    }

    /// <summary>
    /// Replaces the password of the signed-in account. The phone number is not part of this flow:
    /// it is the recovery channel and gets bound while the first password is created.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Change(ChangePasswordViewModel model, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return RedirectToAction(nameof(AccountController.Login), "Account");

        if (model.NewPassword != model.ConfirmPassword)
            return ChangeWithError("رمز عبور و تایید آن یکسان نیستند.");

        var result = await dispatcher.Send(
            new ChangePasswordCommand(
                userId.Value,
                model.CurrentPassword ?? string.Empty,
                model.NewPassword),
            ct);

        if (result.IsFailure)
            return ChangeWithError(result.Error.Description);

        TempData["PasswordMessage"] = "رمز عبور با موفقیت تغییر کرد.";
        return RedirectToAction(nameof(Change));
    }

    /// <summary>
    /// Sends the SMS code proving the signed-in user owns the number being attached to the account.
    /// The user id comes from the cookie only, and the antiforgery token travels as a header.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendPhoneCode([FromBody] SendPhoneCodeViewModel model, CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return Json(new { success = false, error = "برای این کار باید وارد حساب خود شوید." });

        var result = await dispatcher.Send(
            new RequestPhoneBindingCodeCommand(userId.Value, model.PhoneNumber), ct);

        if (result.IsFailure)
            return Json(new { success = false, error = result.Error.Description });

        return Json(new { success = true });
    }

    /// <summary>Redisplays the recovery page with an error toast, keeping the entered number and code.</summary>
    private IActionResult ForgotWithError(string? message, string phoneNumber, string? code = null)
    {
        ViewData["Error"] = message;
        return View("Forgot", new ForgotPasswordViewModel
        {
            PhoneNumber = phoneNumber,
            Code = code,
        });
    }

    /// <summary>Redisplays the first-password form with an error toast, keeping the entered number.</summary>
    private async Task<IActionResult> SetPasswordWithErrorAsync(
        string? message,
        Guid userId,
        SetPasswordViewModel model,
        CancellationToken ct)
    {
        ViewData["Error"] = message;

        var state = await ReadStateAsync(userId, ct);
        return View("SetPassword", new SetPasswordViewModel
        {
            PhoneNumber = model.PhoneNumber,
            RequiresPhoneNumber = state.RequiresPhoneNumber,
        });
    }

    /// <summary>Redisplays the change form with an error toast.</summary>
    private IActionResult ChangeWithError(string? message)
    {
        ViewData["Error"] = message;
        return View("Change", new ChangePasswordViewModel());
    }

    /// <summary>
    /// Reads which signed-in password page the account belongs to. If the read fails the change page
    /// is assumed; both commands re-check the account state server-side either way.
    /// </summary>
    private async Task<PasswordPageStateDto> ReadStateAsync(Guid userId, CancellationToken ct)
    {
        var state = await dispatcher.Send(new GetPasswordPageStateQuery(userId), ct);
        return state is { IsFailure: false }
            ? state.Data!
            : new PasswordPageStateDto(RequiresPhoneNumber: false, HasPassword: true);
    }
}
