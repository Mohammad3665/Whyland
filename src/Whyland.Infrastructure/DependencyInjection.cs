using Whyland.Application.Common.Interfaces.Localization;
using Whyland.Infrastructure.Localization;
using Whyland.Infrastructure.Persistence.Interceptors;
using Whyland.Infrastructure.Persistence.UnitOfWork;
using Whyland.Infrastructure.Services;
using Whyland.Sms.Provider.DependencyInjection;

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

        // Identity helpers.
        services.AddSingleton<IPasswordHashService, BCryptPasswordHasher>();

        // Error wording, read from the localization resources embedded in this assembly. The catalog is
        // immutable once loaded, so one instance serves the whole process.
        services.AddSingleton<IErrorLocalizer, JsonErrorLocalizer>();

        services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();

        // Single-use registration tickets that bridge "verify code" → "complete registration".
        services.AddSingleton<IRegistrationTicketService, RegistrationTicketService>();

        services.AddSmsService(configuration);

        // System clock abstraction (used by the audit interceptor for UTC stamps).
        services.AddSingleton(TimeProvider.System);

        // EF Core interceptors.
        services.AddScoped<AuditableEntityInterceptor>();

        // Shared Npgsql data source (singleton) for low-level PostgreSQL access.
        var dataSource = NpgsqlDataSource.Create(connectionString);
        services.AddSingleton(dataSource);

        return services;
    }

    /// <summary>
    /// Selects the SMS implementation from the "Sms:Provider" setting: "WhylandSms" wires the
    /// HTTP client for the standalone SMS service, anything else keeps the development stub (s11).
    /// </summary>
    private static IServiceCollection AddSmsService(this IServiceCollection services, IConfiguration configuration)
    {
        if (string.Equals(configuration["Sms:Provider"], "WhylandSms", StringComparison.OrdinalIgnoreCase))
        {
            services.AddWhylandSmsProvider(configuration);
            services.AddScoped<ISmsService, WhylandSmsService>();
            return services;
        }

        return services.AddScoped<ISmsService, StubSmsService>();
    }
}