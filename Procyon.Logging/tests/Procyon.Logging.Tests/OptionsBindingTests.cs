using Microsoft.Extensions.Configuration;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Options;

namespace Procyon.Logging.Tests;

public class OptionsBindingTests
{
    [Fact]
    public void ProcyonLoggingOptions_Bind_FromConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Procyon:Logging:Enabled"] = "true",
            ["Procyon:Logging:MinimumLevel"] = "Warning",
            ["Procyon:Logging:File:Path"] = "custom-logs",
            ["Procyon:Logging:File:Mode"] = "Single",
            ["Procyon:Logging:File:SingleFileName"] = "all.json",
            ["Procyon:Logging:File:DateFileFormat"] = "yyyyMMdd",
            ["Procyon:Logging:File:RetentionEnabled"] = "false",
            ["Procyon:Logging:File:RetainDays"] = "9",
            ["Procyon:Logging:Web:Enabled"] = "false",
            ["Procyon:Logging:Web:DevOnly"] = "false",
            ["Procyon:Logging:Web:Path"] = "/logs",
            ["Procyon:Logging:Web:LogRequests"] = "true",
            ["Procyon:Logging:Web:UseSignalR"] = "false",
            ["Procyon:Logging:Web:FallbackPollingSeconds"] = "8",
            ["Procyon:Logging:Web:FaviconPath"] = "/favicon.svg",
            ["Procyon:Logging:ApiLogging:LogRequestBody"] = "true",
            ["Procyon:Logging:ApiLogging:LogResponseBody"] = "true",
            ["Procyon:Logging:ApiLogging:LogHeaders"] = "true",
            ["Procyon:Logging:ApiLogging:LogQueryString"] = "false",
            ["Procyon:Logging:ApiLogging:MaxBodyLength"] = "1024"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var options = new ProcyonLoggingOptions();
        config.GetSection("Procyon:Logging").Bind(options);

        Assert.True(options.Enabled);
        Assert.Equal(ProcyonLogLevel.Warning, options.MinimumLevel);
        Assert.Equal("custom-logs", options.File.Path);
        Assert.Equal(ProcyonLogFileMode.Single, options.File.Mode);
        Assert.Equal("all.json", options.File.SingleFileName);
        Assert.Equal("yyyyMMdd", options.File.DateFileFormat);
        Assert.False(options.File.RetentionEnabled);
        Assert.Equal(9, options.File.RetainDays);
        Assert.False(options.Web.Enabled);
        Assert.False(options.Web.DevOnly);
        Assert.Equal("/logs", options.Web.Path);
        Assert.True(options.Web.LogRequests);
        Assert.False(options.Web.UseSignalR);
        Assert.Equal(8, options.Web.FallbackPollingSeconds);
        Assert.Equal("/favicon.svg", options.Web.FaviconPath);
        Assert.True(options.ApiLogging.LogRequestBody);
        Assert.True(options.ApiLogging.LogResponseBody);
        Assert.True(options.ApiLogging.LogHeaders);
        Assert.False(options.ApiLogging.LogQueryString);
        Assert.Equal(1024, options.ApiLogging.MaxBodyLength);
    }
}
