using System;
using Whyland.Application;
using Whyland.Application.Common.Authorization;
using Whyland.Infrastructure;
using Whyland.Infrastructure.Identity.Constants;

namespace Whyland.Web.Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddInfrastructure(configuration);
        services.AddApplication();
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
