using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Whyland.Application.Common.Abstractions;
using Whyland.Application.Common.Interfaces.Identity;
using Whyland.Infrastructure.Identity.Handlers;
using Whyland.Infrastructure.Identity.Providers;
using Whyland.Infrastructure.Identity.Services;
using Whyland.Infrastructure.Persistence.Interceptors;
using Whyland.Infrastructure.Persistence.UnitOfWork;
using Whyland.Infrastructure.Services;

namespace Whyland.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer.
/// Registers persistence (EF Core + PostgreSQL), the Unit of Work, identity helpers,
/// authorization handlers, and shared infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the Infrastructure services: PostgreSQL persistence (EF Core),
    /// the Unit of Work implementation, identity helpers, and the password hasher.
    /// The connection string must come from configuration and never be hard-coded (s2.9).
    /// </summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configuration">The application configuration used to resolve the connection string.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the required PostgreSQL connection string is missing from configuration.
    /// </exception>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(StaticDataStore.PostgresConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{StaticDataStore.PostgresConnectionStringName}' is not configured. "
                    + "Add ConnectionStrings:Postgres to configuration."
            );

        // EF Core / PostgreSQL.
        services.AddDbContext<WhylandDbContext>((sp, options) =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(3);
                }
            )
            .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>())
        );

        // Unit of Work + transaction handling.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkTransactionHandler, UnitOfWorkTransactionHandler>();

        // Authorization: dynamic permission-based policies.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Identity helpers.
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ICurrentUserService, NullCurrentUserService>();

        // System clock abstraction (used by the audit interceptor for UTC stamps).
        services.AddSingleton(TimeProvider.System);

        // EF Core interceptors.
        services.AddScoped<AuditableEntityInterceptor>();

        // Shared Npgsql data source (singleton) for low-level PostgreSQL access.
        var dataSource = NpgsqlDataSource.Create(connectionString);
        services.AddSingleton(dataSource);

        return services;
    }
}