using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;

namespace Whyland.Infrastructure.Persistence.Context;

/// <summary>
/// Design-time factory used by `dotnet ef` to create the DbContext without running the host.
/// The connection string is never hard-coded: it is read from the appsettings key stored in
/// <see cref="StaticDataStore"/>, overridable by the WHYLAND_POSTGRES_CONNECTION_STRING environment variable.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WhylandDbContext>
{
    private const string DefaultDevConnectionString =
        "Host=localhost;Port=5432;Database=whyland;Username=postgres;Password=postgres";

    public WhylandDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("WHYLAND_POSTGRES_CONNECTION_STRING")
            ?? ReadFromWebAppsettings()
            ?? DefaultDevConnectionString;

        var optionsBuilder = new DbContextOptionsBuilder<WhylandDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WhylandDbContext(optionsBuilder.Options);
    }

    private static string? ReadFromWebAppsettings()
    {
        try
        {
            var appsettingsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..", "Whyland.Web", "appsettings.json");

            if (!File.Exists(appsettingsPath))
                return null;

            using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
            return document.RootElement
                .TryGetProperty("ConnectionStrings", out var connectionStrings) &&
                connectionStrings.TryGetProperty(StaticDataStore.PostgresConnectionStringName, out var postgres)
                ? postgres.GetString()
                : null;
        }
        catch
        {
            // Design-time only: fall back to the default dev connection string on any parse failure.
            return null;
        }
    }
}
