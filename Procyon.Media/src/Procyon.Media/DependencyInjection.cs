using Microsoft.Extensions.DependencyInjection;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Builders;
using Procyon.Media.Resolvers;
using Procyon.Media.Services;
using Procyon.Media.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace Procyon.Media;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcyonMedia(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<MediaOptions>(
            config.GetSection("Procyon:Media"));

        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IMediaPathGenerator, DefaultMediaPathGenerator>();
        services.AddScoped<IMediaUrlResolver, DefaultMediaUrlResolver>();

        return services;
    }
}