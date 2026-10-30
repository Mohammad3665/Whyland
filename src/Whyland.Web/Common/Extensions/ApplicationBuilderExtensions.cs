namespace Whyland.Web.Common.Extensions;

/// <summary>
/// Middleware pipeline helpers for the web application.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline: exception handling and HSTS
    /// outside development, Serilog request logging, static files and
    /// HTTPS redirection, authentication/authorization and MVC/Razor
    /// Pages routing.
    /// </summary>
    /// <param name="app">The web application whose pipeline is configured.</param>
    public static async Task<IApplicationBuilder> UseApplicationMiddlewares(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        // The path is logged without its query string: password reset links carry a live single-use
        // token there, and the log pipeline must not accumulate credentials (s14.2).
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {SafePath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (enricher, httpContext) =>
                enricher.Set("SafePath", httpContext.Request.Path.ToString());
        });

        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapRazorPages().WithStaticAssets();

        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
