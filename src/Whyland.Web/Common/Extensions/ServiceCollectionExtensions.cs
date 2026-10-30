using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Whyland.Application;
using Whyland.Application.Common.Authorization;
using Whyland.Application.Common.Options;
using Whyland.Infrastructure;
using Whyland.Web.Common.ModelBinding;

namespace Whyland.Web.Common.Extensions;

/// <summary>
/// DI registration helpers for the web layer, shared by the host and
/// the MVC/Razor Pages endpoints.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Scheme of the short-lived cookie that holds an external (Google) result while the person
    /// proves a local account. It is deliberately separate from the auth cookie: an external
    /// identity alone must never sign anyone in (s5.4).
    /// </summary>
    public const string ExternalAuthenticationScheme = "Whyland.External";

    /// <summary>Configuration keys holding the Google OAuth client credentials.</summary>
    private const string GoogleClientIdKey = "Auth:GoogleClientId";

    /// <summary>Configuration keys holding the Google OAuth client credentials.</summary>
    private const string GoogleClientSecretKey = "Auth:GoogleClientSecret";

    /// <summary>
    /// Single source of truth for "may we offer Google sign-in". Registration below and the login
    /// page both ask this, so a deployment without keys renders no button instead of an error page.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public static bool HasGoogleCredentials(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration[GoogleClientIdKey]) &&
        !string.IsNullOrWhiteSpace(configuration[GoogleClientSecretKey]);

    /// <summary>
    /// Registers infrastructure and application services, MVC, the HTTP
    /// context accessor, problem details and the AdminPanelAccess
    /// authorization policy (requires the Dashboard.View permission).
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">Application configuration used by the infrastructure registration.</param>
    /// <param name="environment">The hosting environment, used to decide whether the auth cookie should require HTTPS (production) or not (development).
    /// </param>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment
    )
    {
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddPresentation();
        services.AddControllersWithViews();

        // Numbers may be typed with Persian or Arabic-Indic digits anywhere in the UI, so the
        // numeric binders used by Razor Pages and MVC convert them before parsing. Options are
        // shared by both, which is why this is registered once here instead of per Add* call.
        services.Configure<MvcOptions>(options =>
            options.ModelBinderProviders.Insert(0, new EasternDigitsModelBinderProvider()));

        services.AddHttpContextAccessor();

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));
        services.Configure<AdminAccountOptions>(configuration.GetSection(AdminAccountOptions.SectionName));

        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                // Secure only in production; dev runs over plain http://localhost.
                options.Cookie.SecurePolicy = environment.IsDevelopment()
                    ? CookieSecurePolicy.None
                    : CookieSecurePolicy.Always;
            })
            .AddGoogleExternalLogin(configuration, environment);
        // NOTE: Mapster (mapper + config) is registered once by AddMapping inside
        // AddApplication; do not re-register it here or behaviors would double-apply.

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "AdminPanelAccess",
                policy =>
                {
                    policy.RequireClaim(CustomClaimTypes.Permissions, Permissions.Dashboard.View);
                }
            );
        });

        services.AddProblemDetails();

        return services;
    }

    /// <summary>
    /// Registers Google as an external login provider, but only when its keys are configured —
    /// a deployment without them keeps working and simply renders no Google button (s5.4).
    /// </summary>
    /// <param name="builder">The authentication builder already holding the cookie scheme.</param>
    /// <param name="configuration">Reads Auth:GoogleClientId and Auth:GoogleClientSecret.</param>
    /// <param name="environment">Decides whether the temporary external cookie may run over plain http.</param>
    private static AuthenticationBuilder AddGoogleExternalLogin(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var clientId = configuration[GoogleClientIdKey];
        var clientSecret = configuration[GoogleClientSecretKey];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return builder;

        builder.AddCookie(ExternalAuthenticationScheme, options =>
        {
            options.Cookie.Name = "whyland.external";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.None
                : CookieSecurePolicy.Always;

            // Long enough to finish the OTP step, short enough that an abandoned Google
            // redirect cannot be picked up much later.
            options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            options.SlidingExpiration = false;
        });

        builder.AddOAuth("Google", options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = "/signin-google";
            options.SignInScheme = ExternalAuthenticationScheme;

            options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
            options.TokenEndpoint = "https://oauth2.googleapis.com/token";
            options.UserInformationEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";

            options.Scope.Add("openid");
            options.Scope.Add("email");
            options.Scope.Add("profile");

            // The subject id is the only stable identifier; names are pre-filled hints, never proofs.
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
            options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
            options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "given_name");
            options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");

            // .NET 10's OAuth handler no longer queries the user-information endpoint itself — it
            // hands OnCreatingTicket an empty JSON document, so the ClaimActions above would map
            // nothing and the principal would carry no subject id. Fetch the profile here, using the
            // handler's own backchannel, and run the mappings against the response.
            options.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        context.TokenResponse.AccessToken
                    );

                    using var response = await context.Backchannel.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        context.HttpContext.RequestAborted
                    );

                    // Without a profile there is no subject id: leave the identity empty and let the
                    // callback refuse the sign-in (fail closed) rather than invent one.
                    if (!response.IsSuccessStatusCode)
                        return;

                    using var user = JsonDocument.Parse(
                        await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted)
                    );

                    context.RunClaimActions(user.RootElement);
                }
            };

            // Access tokens are not needed — the link stores only the subject id.
            options.SaveTokens = false;
        });

        return builder;
    }
}
