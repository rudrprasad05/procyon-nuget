using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Procyon.Email.Azure.Configuration;
using Procyon.Email.Azure.Internal;
using Procyon.Email.Azure.Mapping;
using Procyon.Email.Azure.Providers;
using Procyon.Email.Azure.Validation;
using Procyon.Email.Configuration;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;

namespace Procyon.Email.Azure.DependencyInjection;

/// <summary>
/// Registers the Azure Communication Services provider for Procyon.Email.
/// </summary>
public static class AzureEmailBuilderExtensions
{
    /// <summary>
    /// Adds the Azure Communication Services email provider to a Procyon.Email registration.
    /// </summary>
    /// <param name="builder">The Procyon.Email builder.</param>
    /// <returns>The current builder.</returns>
    public static IProcyonEmailBuilder UseAzure(this IProcyonEmailBuilder builder)
    {
        builder.Services.AddOptions<AzureEmailOptions>()
            .Bind(builder.Configuration.GetSection(AzureEmailOptions.SectionPath))
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<AzureEmailOptions>, AzureEmailOptionsValidator>();
        builder.Services.AddSingleton<AzureApiKeyProvider>();
        builder.Services.AddSingleton<AzureRequestMapper>();
        builder.Services.AddSingleton<AzureResponseMapper>();
        builder.Services.AddHttpClient<AzureEmailProvider>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AzureEmailOptions>>().Value;
            var emailOptions = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
            var connectionDetails = AzureConnectionString.TryParse(options.ConnectionString)
                ?? throw new InvalidOperationException("Procyon:Email:Azure:ConnectionString must contain Endpoint and AccessKey values.");

            httpClient.BaseAddress = new Uri(options.ApiBaseUrl ?? connectionDetails.Endpoint.ToString(), UriKind.Absolute);
            httpClient.Timeout = TimeSpan.FromSeconds(emailOptions.Delivery.TimeoutSeconds);
        });
        builder.Services.AddTransient<IEmailProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<AzureEmailProvider>());

        return builder;
    }

    /// <summary>
    /// Adds the Azure Communication Services email provider to a Procyon.Email registration.
    /// </summary>
    /// <param name="builder">The Procyon.Email builder.</param>
    /// <returns>The current builder.</returns>
    public static IProcyonEmailBuilder AddAzureEmailProvider(this IProcyonEmailBuilder builder)
    {
        return builder.UseAzure();
    }
}
