using System;

namespace Whyland.Web.Common.Extensions;

public static class ApplicationBuilderExtensions
{
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
