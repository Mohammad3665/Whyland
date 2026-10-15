using System;
using Whyland.Application.Common.Mappings;
using Whyland.Web.Common.Extensions;

namespace Whyland.Web;

public class StartupHost
{
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
