using Microsoft.AspNetCore.Authentication;

namespace Whyland.Sms.Controllers;

/// <summary>
/// Sign-in and sign-out for the dashboard. The credentials are configuration values, not
/// users: the simulator has no account store, and the panel exists to protect OTP text
/// rather than to model people.
/// </summary>
/// <param name="options">Dashboard credentials.</param>
[AllowAnonymous]
public class AccountController(IOptions<SmsServiceOptions> options) : Controller
{
    /// <summary>Shows the sign-in form, or a configuration notice when no password exists.</summary>
    [HttpGet]
    public IActionResult Login() =>
        View(new LoginPanelViewModel { CredentialsConfigured = PanelAuthentication.IsConfigured(options.Value) });

    /// <summary>Verifies the pair against configuration and issues the panel cookie.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginPanelViewModel model)
    {
        model.CredentialsConfigured = PanelAuthentication.IsConfigured(options.Value);

        if (!PanelAuthentication.TryVerify(options.Value, model.Username, model.Password))
        {
            // One generic message: the form must not reveal which half was wrong.
            ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور صحیح نیست.");
            model.Password = null;
            return View(model);
        }

        await HttpContext.SignInAsync(
            PanelAuthentication.Scheme,
            PanelAuthentication.CreatePrincipal(model.Username!),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(PanelAuthentication.SessionLength),
            });

        return RedirectToAction("Index", "Dashboard");
    }

    /// <summary>Drops the panel cookie.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(PanelAuthentication.Scheme);
        return RedirectToAction(nameof(Login));
    }
}
