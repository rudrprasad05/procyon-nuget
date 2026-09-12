using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Azure;

namespace Procyon.Media.Tests;

public sealed class AzureServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureMediaProvider_RegistersMediaProvider()
    {
        var configuration = new ConfigurationManager
        {
            ["ConnectionStrings:AzureBlob"] = "UseDevelopmentStorage=true",
            ["Media:Container"] = "media"
        };
        var services = new ServiceCollection();

        services.AddAzureMediaProvider(configuration);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IMediaProvider));
    }

    [Fact]
    public void AddAzureMediaProvider_RequiresConnectionString()
    {
        var configuration = new ConfigurationManager
        {
            ["Media:Container"] = "media"
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddAzureMediaProvider(configuration));

        Assert.Contains("AzureBlob", exception.Message);
    }

    [Fact]
    public void AddAzureMediaProvider_RequiresContainer()
    {
        var configuration = new ConfigurationManager
        {
            ["ConnectionStrings:AzureBlob"] = "UseDevelopmentStorage=true"
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().AddAzureMediaProvider(configuration));

        Assert.Contains("Media:Container", exception.Message);
    }
}
