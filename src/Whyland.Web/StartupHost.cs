using System;
using Whyland.Application.Common.Mappings;
using Whyland.Web.Common.Extensions;

namespace Whyland.Web;

/// <summary>
/// Composition root of the web host: builds the WebApplication with the
/// admin Razor Pages root, application services and Serilog logging.
/// </summary>
public class StartupHost
{
    /// <summary>
    /// Creates the application builder, registers Razor Pages (rooted at
    /// /Areas/Admin/Pages), mapping, application services and Serilog,
    /// then builds and returns the WebApplication.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to the host builder.</param>
    public static WebApplication Build(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages().WithRazorPagesRoot("/Areas/Admin/Pages");
        builder.Services.AddMapping();

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Host.UseSerilog(
            (context, services, logger) =>
            {
                logger
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            }
        );

        return builder.Build();
    }
}
