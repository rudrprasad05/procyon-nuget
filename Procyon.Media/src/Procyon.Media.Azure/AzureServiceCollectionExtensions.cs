using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Azure.Providers;

namespace Procyon.Media.Azure;

public static class AzureServiceCollectionExtensions
{
    public static IServiceCollection AddAzureMediaProvider(
        this IServiceCollection services,
        IConfiguration config)
    {
        var connectionString = config.GetConnectionString("AzureBlob");
        var containerName = config["Media:Container"];

        services.AddSingleton(_ =>
        {
            var serviceClient = new BlobServiceClient(connectionString);
            var container = serviceClient.GetBlobContainerClient(containerName);

            container.CreateIfNotExists();
            return container;
        });

        services.AddScoped<IMediaProvider, AzureBlobMediaProvider>();

        return services;
    }
}