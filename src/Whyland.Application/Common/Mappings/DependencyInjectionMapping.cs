using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Whyland.Application.Common.Mappings;

/// <summary>
/// Registers Mapster object mapping services.
/// </summary>
public static class DependencyInjectionMapping
{
    /// <summary>
    /// Adds Mapster configuration and mapping services.
    /// Scans this assembly for mapping registrations (<see cref="MappingConfig"/>),
    /// registers <see cref="TypeAdapterConfig"/> as a singleton and
    /// <see cref="IMapper"/> (ServiceMapper) as a scoped service.
    /// Services registered as scoped so that the injected IMapper resolves the current
    /// configuration at each request, supporting later runtime registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddMapping(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.Scan(typeof(DependencyInjectionMapping).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
