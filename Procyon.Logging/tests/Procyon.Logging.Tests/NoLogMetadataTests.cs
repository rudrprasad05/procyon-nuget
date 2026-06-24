using Microsoft.AspNetCore.Http;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Middleware;

namespace Procyon.Logging.Tests;

public class NoLogMetadataTests
{
    [Fact]
    public void HasNoLog_ReturnsTrue_WhenEndpointHasNoLogAttribute()
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new NoLogAttribute()),
            "no-log-endpoint");

        Assert.True(NoLogMetadata.HasNoLog(endpoint));
    }

    [Fact]
    public void HasNoLog_ReturnsFalse_WhenEndpointIsMissingAttribute()
    {
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            EndpointMetadataCollection.Empty,
            "logged-endpoint");

        Assert.False(NoLogMetadata.HasNoLog(endpoint));
    }
}
