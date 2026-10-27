using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Whyland.Application.Common.Abstractions.CQRS.Dispatchers;
using Whyland.Application.Common.Validation;
using Whyland.Application.Features.Auth.ExternalLogin;
using Whyland.Application.Features.Auth.Login.LoginWithPassword;
using Whyland.Application.Features.Auth.Login.RequestLoginCode;
using Whyland.Application.Features.Auth.Login.VerifyLoginCode;
using Whyland.Application.Features.Auth.Registration.CompleteRegistration;
using Whyland.Shared.Dtos.Identity;
using Whyland.Web.Common.Identity;
using Whyland.Web.ViewModels;

namespace Whyland.Web.Controllers;

/// <summary>
/// The public, anonymous sign-in / registration flow (s5.1.4, s5.3).
/// <para>
/// <b>One page, three steps, one URL.</b> The browser stays on <c>/Account/Login</c> for the whole
/// flow: the page's JavaScript calls the three JSON endpoints below and toggles the steps
/// client-side, exactly like the raw template. No round-trip ever navigates away from the page
/// (the only navigation is the final redirect to the public index after a successful sign-in).
/// </para>
/// <list type="number">
///   <item><description><c>SendCode</c> — step 1: request a login code for the phone number.</description></item>
///   <item><description><c>VerifyCode</c> — step 2: verify it. Existing account → cookie + redirect to
///   index; new phone → a single-use registration ticket for step 3.</description></item>
///   <item><description><c>CompleteRegistration</c> — step 3: create the account from the ticket → cookie + redirect.</description></item>
/// </list>
/// The endpoints carry no antiforgery token (the page has none by design), so CSRF is enforced
/// with a same-origin check on Origin/Referer; the auth cookie itself is SameSite=Lax.
/// </summary>
[AllowAnonymous]
public class AccountController(IDispatcher dispatcher, ILogger<AccountController> logger) : Controller
{
    /// <summary>The only external provider wired so far (s5.4).</summary>
    private const string GoogleProvider = "Google";
    /// <summary>Renders the single login page (step 1 shown; steps 2 and 3 are toggled by JS).</summary>
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    /// <summary>Step 1 — issues and sends a login code. Returns the normalized phone so every
    /// later step talks about the exact value stored in the database.</summary>
    [HttpPost]
    public async Task<IActionResult> SendCode([FromBody] LoginViewModel model, CancellationToken ct)
    {
        if (!IsSameOriginRequest())
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, error = "درخواست نامعتبر است." });

        if (model is null || string.IsNullOrWhiteSpace(model.PhoneNumber))
            return Json(new { success = false, error = "شماره موبایل را وارد کنید." });

        var phone = Normalize(model.PhoneNumber);
        var result = await dispatcher.Send(new RequestLoginCodeCommand(phone), ct);
        if (result.IsFailure)
            return Json(new { success = false, error = result.Error.Description ?? "خطا در ارسال کد." });

        return Json(new { success = true, phoneNumber = phone });
    }

    /// <summary>Step 2 — verifies the code. Existing, active account → signs in and returns the
    /// index as redirect target; brand-new phone → returns a registration ticket (no navigation).</summary>
    [HttpPost]
    public async Task<IActionResult> VerifyCode([FromBody] LoginViewModel model, CancellationToken ct)
    {
        if (!IsSameOriginRequest())
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, error = "درخواست نامعتبر است." });

        if (model is null || string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Code))
            return Json(new { success = false, error = "کد تأیید را وارد کنید." });

        var result = await dispatcher.Send(
            new VerifyLoginCodeCommand(model.PhoneNumber.Trim(), model.Code.Trim()), ct);

        if (result.IsFailure)
            return Json(new { success = false, error = result.Error.Description ?? "کد نامعتبر است." });

        var outcome = result.Data!;

        // Existing account → cookie issued; the page then navigates to the index once.
        if (outcome.User is not null)
        {
            await SignInAsync(outcome.User);
            return Json(new { success = true, requiresRegistration = false, redirectUrl = "/" });
        }

        // New phone → stay on the page and advance to step 3 with the ticket.
        return Json(new { success = true, requiresRegistration = true, ticket = outcome.RegistrationTicket });
    }

    /// <summary>Step 3 — creates the account from the ticket and signs the user in.</summary>
    [HttpPost]
    public async Task<IActionResult> CompleteRegistration([FromBody] LoginViewModel model, CancellationToken ct)
    {
        if (!IsSameOriginRequest())
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, error = "درخواست نامعتبر است." });

        if (model is null || string.IsNullOrWhiteSpace(model.Ticket))
            return Json(new { success = false, error = "درخواست ثبت‌نام نامعتبر است." });

        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
            return Json(new { success = false, error = "نام و نام خانوادگی را وارد کنید." });

        var result = await dispatcher.Send(
            new CompleteRegistrationCommand(
                model.Ticket,
                model.FirstName.Trim(),
                model.LastName.Trim(),
                model.Password),
            ct);

        if (result.IsFailure)
        {
            // Stale/expired ticket → the page restarts the flow at step 1 (still same URL).
            if (result.Error.Code == "Auth.InvalidTicket")
                return Json(new { success = false, error = result.Error.Description, restart = true });

            return Json(new { success = false, error = result.Error.Description ?? "خطا در ثبت‌نام." });
        }

        await SignInAsync(result.Data!);
        return Json(new { success = true, redirectUrl = "/" });
    }

    /// <summary>Password tab — signs in with the locally stored password, by phone number only (s5.1.1).</summary>
    [HttpPost]
    public async Task<IActionResult> LoginWithPassword([FromBody] LoginViewModel model, CancellationToken ct)
    {
        if (!IsSameOriginRequest())
            return StatusCode(StatusCodes.Status403Forbidden, new { success = false, error = "درخواست نامعتبر است." });

        if (model is null || string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Password))
            return Json(new { success = false, error = "شماره موبایل و رمز عبور را وارد کنید." });

        var result = await dispatcher.Send(
            new LoginWithPasswordCommand(model.PhoneNumber, model.Password), ct);

        if (result.IsFailure)
            return Json(new { success = false, error = result.Error.Description ?? "ورود ناموفق بود." });

        await SignInAsync(result.Data!);
        return Json(new { success = true, redirectUrl = "/" });
    }

    /// <summary>
    /// Starts the Google round trip. The provider never signs anyone in directly: it produces a
    /// temporary external principal that <see cref="ExternalCallback"/> resolves to a local account
    /// (s5.4).
    /// </summary>
    /// <param name="provider">Authentication scheme to challenge, currently always "Google".</param>
    [HttpGet]
    public IActionResult ExternalChallenge(string provider = GoogleProvider)
    {
        if (!string.Equals(provider, GoogleProvider, StringComparison.Ordinal))
            return NotFound();

        return Challenge(new AuthenticationProperties { RedirectUri = "/Account/ExternalCallback" }, provider);
    }

    /// <summary>
    /// Receives the external principal after Google redirected back and signs in the account it
    /// resolves to — an existing link, or a fresh phone-less one created on this first visit (s5.4).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExternalCallback(CancellationToken ct)
    {
        var external = await HttpContext.AuthenticateAsync(ServiceCollectionExtensions.ExternalAuthenticationScheme);

        if (!external.Succeeded || external.Principal is null)
        {
            // Google did redirect back to us, so the round trip worked and this is about the
            // short-lived ticket: without a line here the whole flow looks like a silent no-op.
            logger.LogWarning(
                "External sign-in ticket could not be read: {Failure}",
                external.Failure?.Message ?? "no ticket cookie was presented");

            return RedirectToAction(nameof(Login));
        }

        // The temporary principal is dropped as soon as it has been read; it must not survive
        // in a cookie that a later request could replay.
        await HttpContext.SignOutAsync(ServiceCollectionExtensions.ExternalAuthenticationScheme);

        var identity = new ExternalIdentityDto(
            Provider: GoogleProvider,
            ProviderKey: external.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            Email: external.Principal.FindFirstValue(ClaimTypes.Email),
            FirstName: external.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName: external.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty);

        if (string.IsNullOrEmpty(identity.ProviderKey))
        {
            // Only the claim types are logged — never the values.
            logger.LogWarning(
                "External principal carried no subject id; claims received: {Claims}",
                string.Join(", ", external.Principal.Claims.Select(c => c.Type)));

            return RedirectToAction(nameof(Login));
        }

        var result = await dispatcher.Send(new ExternalLoginCallbackCommand(identity), ct);

        if (result.IsFailure)
        {
            logger.LogWarning(
                "External sign-in rejected ({ErrorCode}): {ErrorDescription}",
                result.Error.Code,
                result.Error.Description);

            return RedirectToAction(nameof(Login));
        }

        await SignInAsync(result.Data!);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Signs the user out and returns to the public index (s5.3).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Issues the auth cookie from a successful login result.</summary>
    private async Task SignInAsync(AuthenticatedUserDto user)
    {
        var principal = ClaimsPrincipalFactory.Create(user);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = true });
    }

    /// <summary>
    /// CSRF guard for the token-less JSON endpoints: the Origin header (or Referer as
    /// fallback) must point back at this same host. Requests carrying neither are rejected.
    /// </summary>
    private bool IsSameOriginRequest()
    {
        var origin = Request.Headers.Origin.FirstOrDefault();
        if (!string.IsNullOrEmpty(origin))
            return IsThisHost(origin);

        var referer = Request.Headers.Referer.FirstOrDefault();
        if (!string.IsNullOrEmpty(referer))
            return IsThisHost(referer);

        return false;
    }

    /// <summary>True when the given absolute URL targets the host serving this request.</summary>
    private bool IsThisHost(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase);

    /// <summary>Normalizes Persian/international formats to 09xxxxxxxxx (idempotent; the same
    /// helper the command handlers use, so one number has exactly one spelling everywhere).</summary>
    private static string Normalize(string phone) => PhoneNormalizer.Normalize(phone);
}
