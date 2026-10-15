using Microsoft.Extensions.DependencyInjection;
using Whyland.Infrastructure.Common.Extensions;
using Whyland.Infrastructure.Persistence.Seeders;

namespace Whyland.Infrastructure.Persistence;

/// <summary>
/// Initializes the database with migrations and seed data.
/// The production database migration strategy must not rely on manual database editing (s3.8).
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations and seeds initial data.
    /// Safe to call repeatedly: migrations are idempotent and seeding is guarded by existence checks.
    /// </summary>
    /// <param name="serviceProvider">The service provider (typically the app's root provider).</param>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<WhylandDbContext>();

        scope.ServiceProvider.ApplyMigrations();

        await context.SeedDatabaseAsync();
    }
}
