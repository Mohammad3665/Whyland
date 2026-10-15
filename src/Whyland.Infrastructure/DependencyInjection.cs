using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whyland.Application.Common.Abstractions;
using Whyland.Infrastructure.Identity.Handlers;
using Whyland.Infrastructure.Identity.Providers;
using Whyland.Infrastructure.Persistence.UnitOfWork;
using Whyland.Infrastructure.Services;

namespace Whyland.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the Infrastructure services: PostgreSQL persistence (EF Core),
    /// the Unit of Work implementation, and the password hasher.
    /// The connection string must come from configuration and never be hard-coded (s2.9).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(StaticDataStore.PostgresConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{StaticDataStore.PostgresConnectionStringName}' is not configured. "
                    + "Add ConnectionStrings:Postgres to configuration."
            );

        services.AddDbContext<WhylandDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                }
            )
        );

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkTransactionHandler, UnitOfWorkTransactionHandler>();

        // Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        return services;
    }
}
