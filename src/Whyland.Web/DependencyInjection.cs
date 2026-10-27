using Whyland.Application.Common.Interfaces.Identity;
using Whyland.Application.Common.Interfaces.Services;
using Whyland.Infrastructure.Identity.Handlers;
using Whyland.Web.Common.Identity.Providers;
using Whyland.Web.Common.Identity.Services;

namespace Whyland.Web;

/// <summary>
/// Composition root for the Presentation layer.
/// Registers presentation-only services such as the dynamic permission-based
/// authorization policy provider and the permission authorization handler.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers presentation-layer services: authorization handlers and
    /// the dynamic permission-based policy provider.
    /// </summary>
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // Authorization: dynamic permission-based policies.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<CurrentUserAvatarService>();

        return services;
    }
}