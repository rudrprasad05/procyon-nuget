using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Middleware;
using Procyon.Logging.Options;
using Procyon.Logging.Services;
using Procyon.Logging.Web;

namespace Procyon.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddProcyonLogging(
        this IServiceCollection services,
        IConfiguration config)
    {
        var section = config.GetSection("Procyon:Logging");
        var configuredOptions = new ProcyonLoggingOptions();
        section.Bind(configuredOptions);

        services.Configure<ProcyonLoggingOptions>(section);

        services.AddSingleton<ProcyonLogQueue>();
        services.AddSingleton<IProcyonLogger, ProcyonLogger>();
        services.AddSingleton<ProcyonFileLogWriter>();
        services.AddSingleton<ProcyonLogRetentionService>();
        services.AddSingleton<ProcyonLogStore>();
        services.AddHostedService<ProcyonLogWriterHostedService>();
        services.AddHostedService<ProcyonLogRetentionHostedService>();

        if (configuredOptions.Web.Enabled && configuredOptions.Web.UseSignalR)
        {
            services.AddSignalR()
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            services.AddSingleton<IProcyonLogBroadcaster, SignalRProcyonLogBroadcaster>();
        }
        else
        {
            services.AddSingleton<IProcyonLogBroadcaster, NullProcyonLogBroadcaster>();
        }

        return services;
    }

    public static IApplicationBuilder UseProcyonLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<ProcyonLoggingMiddleware>();

        if (app is IEndpointRouteBuilder endpoints)
            endpoints.MapProcyonLogs();

        return app;
    }
}
