using Microsoft.AspNetCore.Authentication.Cookies;
using Whyland.Application;
using Whyland.Application.Common.Authorization;
using Whyland.Application.Common.Options;
using Whyland.Infrastructure;
using Whyland.Web.Common.Identity.Constants;

namespace Whyland.Web.Common.Extensions;

/// <summary>
/// DI registration helpers for the web layer, shared by the host and
/// the MVC/Razor Pages endpoints.
/// </summary>
public static class ServiceCollectionExtensions
{
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
        services.AddHttpContextAccessor();

        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.SectionName));

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
            });
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
}
