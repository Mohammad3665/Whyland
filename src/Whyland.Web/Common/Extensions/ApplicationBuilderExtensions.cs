using System;

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
        app.UseSerilogRequestLogging();

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
