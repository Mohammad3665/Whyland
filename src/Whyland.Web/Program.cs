using Whyland.Infrastructure.Persistence;
using Whyland.Web;
using Whyland.Web.Common.Extensions;

try
{
    var app = StartupHost.Build(args);
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
