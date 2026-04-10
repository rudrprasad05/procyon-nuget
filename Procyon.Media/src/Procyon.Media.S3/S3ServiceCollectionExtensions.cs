using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.S3.Providers;
using Microsoft.Extensions.DependencyInjection;
namespace Procyon.Media.S3;


public static class S3ServiceCollectionExtensions
{
    public static IServiceCollection AddS3Provider(
        this IServiceCollection services,
        IConfiguration config)
    {
        var bucket = config["Procyon:Media:S3:Bucket"];

        services.AddAWSService<IAmazonS3>();

        services.AddScoped<IMediaProvider>(sp =>
        {
            var s3 = sp.GetRequiredService<IAmazonS3>();
            return new S3MediaProvider(s3, bucket!);
        });

        return services;
    }
}