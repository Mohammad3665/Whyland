using Whyland.Application;
using Whyland.Application.Common.Authorization;
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
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddPresentation();
        services.AddControllersWithViews();
        services.AddHttpContextAccessor();
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
