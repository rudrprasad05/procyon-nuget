using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Procyon.Logging.Options;

namespace Procyon.Logging.Tests;

public class ProcyonLoggingUiTests
{
    [Fact]
    public void WebOptions_DefaultPath_UsesLoggingPath()
    {
        var options = new ProcyonLoggingWebOptions();

        Assert.Equal("/procyon/logging", options.Path);
        Assert.Equal("/procyon/logging/favicon.svg", options.FaviconPath);
    }

    [Fact]
    public void UseProcyonLoggingUi_MapsConfiguredLoggingEndpoints()
    {
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Procyon:Logging:Web:Path"] = "procyon/custom-logs",
            ["Procyon:Logging:Web:FaviconPath"] = "custom-favicon.svg",
            ["Procyon:Logging:Web:UseSignalR"] = "false"
        });
        var app = BuildApp(config);

        app.UseProcyonLoggingUi();

        var patterns = GetRoutePatterns(app);
        Assert.Contains("/procyon/custom-logs", patterns);
        Assert.Contains("/procyon/custom-logs/entries", patterns);
        Assert.Contains("/custom-favicon.svg", patterns);
    }

    [Fact]
    public void UseProcyonLogging_AndUseProcyonLoggingUi_MapUiOnce()
    {
        var config = BuildConfig(new Dictionary<string, string?>
        {
            ["Procyon:Logging:Web:UseSignalR"] = "false"
        });
        var app = BuildApp(config);

        app.UseProcyonLogging();
        app.UseProcyonLoggingUi();

        var patterns = GetRoutePatterns(app);
        Assert.Single(patterns.Where(pattern => pattern == "/procyon/logging"));
        Assert.Single(patterns.Where(pattern => pattern == "/procyon/logging/entries"));
        Assert.Single(patterns.Where(pattern => pattern == "/procyon/logging/favicon.svg"));
    }

    private static IConfiguration BuildConfig(Dictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

    private static WebApplication BuildApp(IConfiguration config)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddProcyonLogging(config);
        return builder.Build();
    }

    private static IReadOnlyList<string> GetRoutePatterns(WebApplication app)
        => ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(pattern => pattern is not null)
            .Select(pattern => pattern!)
            .ToArray();
}
