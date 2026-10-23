using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Whyland.Application.Common.Abstractions.CQRS.Dispatchers;
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
public class AccountController(IDispatcher dispatcher) : Controller
{
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
            new CompleteRegistrationCommand(model.Ticket, model.FirstName.Trim(), model.LastName.Trim(), model.Password),
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

    /// <summary>Normalizes Persian/international formats to 09xxxxxxxxx (idempotent; matches
    /// the normalization inside RequestLoginCodeCommandHandler).</summary>
    private static string Normalize(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("98")) return "0" + digits[2..];
        if (digits.StartsWith("0098")) return "0" + digits[4..];
        return digits;
    }
}
