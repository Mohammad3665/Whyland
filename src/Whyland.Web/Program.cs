using Whyland.Infrastructure.Persistence;
using Whyland.Web;

try
{
    var app = StartupHost.Build(args);

    // A clone without Google keys simply shows no Google button; say so once at startup so that
    // is not mistaken for a broken build.
    if (!ServiceCollectionExtensions.HasGoogleCredentials(app.Configuration))
    {
        app.Logger.LogInformation(
            "Google sign-in is disabled: set Auth:GoogleClientId and Auth:GoogleClientSecret " +
            "(see Documentation/Whyland-Auth-Setup.md).");
    }

    await app.UseApplicationMiddlewares();
    await app.Services.InitializeDatabaseAsync();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unexpected error occurred. Application is shutting down.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
