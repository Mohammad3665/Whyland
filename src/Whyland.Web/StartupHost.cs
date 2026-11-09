using Whyland.Application.Common.Mappings;

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

        builder.Services.AddRazorPages(options =>
        {
            // The pages are routed as an area (/Admin/Users), so the gate has to be an area
            // convention: AuthorizeFolder("/") is matched against a folder path and never bites here,
            // which left every admin page anonymous.
            options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPanelAccess");
        }).WithRazorPagesRoot("/Areas/Admin/Pages");
        builder.Services.AddControllersWithViews();
        builder.Services.AddMapping();

        // Kestrel's default body limit (30 MB) is below the configured video ceiling; the form limit
        // is raised in the same place, from the same configuration.
        var maxUploadBytes = Whyland.Web.Common.Extensions.ServiceCollectionExtensions
            .MaxUploadBytes(builder.Configuration);
        builder.WebHost.ConfigureKestrel(options =>
            options.Limits.MaxRequestBodySize = maxUploadBytes);

        builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);
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
