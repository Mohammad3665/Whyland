using System.Text.Json;
using Microsoft.EntityFrameworkCore.Design;

namespace Whyland.Infrastructure.Persistence.Context;

/// <summary>
/// Design-time factory used by `dotnet ef` to create the DbContext without running the host.
/// The connection string is read in this order:
/// 1. the WHYLAND_POSTGRES_CONNECTION_STRING environment variable;
/// 2. the ConnectionStrings section in Whyland.Web/appsettings.json;
/// 3. the ConnectionStrings section in Whyland.Web/appsettings.Development.json.
/// If no connection string can be found, an InvalidOperationException is thrown.
/// Note: user secrets are intentionally not read here; when the password is stored only
/// in user secrets, set the WHYLAND_POSTGRES_CONNECTION_STRING environment variable.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<WhylandDbContext>
{
    /// <summary>
    /// Creates a <see cref="WhylandDbContext"/> configured with the design-time connection string
    /// resolved from the environment or Whyland.Web configuration files.
    /// </summary>
    /// <param name="args">Command-line arguments passed by the design-time tooling (unused).</param>
    /// <returns>A configured <see cref="WhylandDbContext"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no connection string can be resolved.</exception>
    public WhylandDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("WHYLAND_POSTGRES_CONNECTION_STRING")
            ?? ReadFromWebAppsettings()
            ?? throw new InvalidOperationException(
                "Connection string not found. Set the WHYLAND_POSTGRES_CONNECTION_STRING "
                    + "environment variable or configure ConnectionStrings:Postgres in "
                    + "Whyland.Web/appsettings.json."
            );

        var optionsBuilder = new DbContextOptionsBuilder<WhylandDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new WhylandDbContext(optionsBuilder.Options);
    }

    private static string? ReadFromWebAppsettings()
    {
        foreach (var fileName in new[] { "appsettings.json", "appsettings.Development.json" })
        {
            var connectionString = ReadConnectionString(fileName);
            if (connectionString is not null)
                return connectionString;
        }

        return null;
    }

    private static string? ReadConnectionString(string fileName)
    {
        try
        {
            var appsettingsPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "Whyland.Web",
                fileName
            );

            if (!File.Exists(appsettingsPath))
                return null;

            using var document = JsonDocument.Parse(File.ReadAllText(appsettingsPath));
            return
                document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                && connectionStrings.TryGetProperty(
                    StaticDataStore.PostgresConnectionStringName,
                    out var postgres
                )
                ? postgres.GetString()
                : null;
        }
        catch
        {
            // Design-time only: ignore files that cannot be read or parsed and keep checking
            // the next source instead of aborting tooling.
            return null;
        }
    }
}
