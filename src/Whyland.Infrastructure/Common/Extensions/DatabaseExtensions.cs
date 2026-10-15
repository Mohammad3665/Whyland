using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Whyland.Infrastructure.Common.Extensions;

/// <summary>
/// Provides database migration extension methods.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Applies pending migrations to the database.
    /// </summary>
    /// <param name="services">The service provider.</param>
    /// <remarks>
    /// Creates a scoped service, retrieves the database context, and applies any pending migrations.
    /// Logs success or failure with detailed error information.
    /// </remarks>
    public static void ApplyMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope
            .ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DB Migration");

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var defaultConnection = configuration.GetConnectionString(
            StaticDataStore.PostgresConnectionStringName
        );

        var builder = new NpgsqlConnectionStringBuilder(defaultConnection);
        var databaseName = builder.Database;

        builder.Database = "postgres";
        var masterConnection = builder.ToString();

        try
        {
            logger.LogInformation("Ensuring database '{DatabaseName}' exists...", databaseName);
            var retries = 0;
            const int maxRetries = 30;

            while (true)
            {
                try
                {
                    using var connection = new NpgsqlConnection(masterConnection);
                    connection.Open();

                    using var checkCmd = new NpgsqlCommand(
                        "SELECT 1 FROM pg_database WHERE datname = @name",
                        connection
                    );
                    checkCmd.Parameters.AddWithValue("name", databaseName ?? string.Empty);

                    var exists = checkCmd.ExecuteScalar() != null;

                    if (!exists)
                    {
                        var quotedDbName = $"\"{databaseName!.Replace("\"", "\"\"")}\"";
                        using var createCmd = new NpgsqlCommand(
                            $"CREATE DATABASE {quotedDbName}",
                            connection
                        );
                        createCmd.ExecuteNonQuery();

                        logger.LogInformation("Database '{DatabaseName}' created.", databaseName);
                    }
                    else
                    {
                        logger.LogInformation(
                            "Database '{DatabaseName}' already exists.",
                            databaseName
                        );
                    }

                    break;
                }
                catch (Exception ex)
                {
                    retries++;
                    if (retries >= maxRetries)
                    {
                        logger.LogError(
                            ex,
                            "Database server did not respond in time. Migration aborted."
                        );
                        return;
                    }

                    logger.LogWarning(
                        ex,
                        "Database server not ready yet ({Retries}/{MaxRetries})...",
                        retries,
                        maxRetries
                    );

                    Thread.Sleep(5000);
                }
            }

            var dbOptions = new DbContextOptionsBuilder<WhylandDbContext>()
                .UseNpgsql(defaultConnection)
                .Options;

            using var context = new WhylandDbContext(dbOptions);
            context.Database.Migrate();

            logger.LogInformation("Database migrated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed.");
            throw;
        }
    }
}
