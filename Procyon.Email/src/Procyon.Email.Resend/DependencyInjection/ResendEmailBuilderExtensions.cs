using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Procyon.Email.Configuration;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;
using Procyon.Email.Resend.Configuration;
using Procyon.Email.Resend.Internal;
using Procyon.Email.Resend.Mapping;
using Procyon.Email.Resend.Providers;
using Procyon.Email.Resend.Validation;

namespace Procyon.Email.Resend.DependencyInjection;

/// <summary>
/// Registers the Resend provider for Procyon.Email.
/// </summary>
public static class ResendEmailBuilderExtensions
{
    /// <summary>
    /// Adds the Resend email provider to a Procyon.Email registration.
    /// </summary>
    /// <param name="builder">The Procyon.Email builder.</param>
    /// <returns>The current builder.</returns>
    public static IProcyonEmailBuilder UseResend(this IProcyonEmailBuilder builder)
    {
        builder.Services.AddOptions<ResendEmailOptions>()
            .Bind(builder.Configuration.GetSection(ResendEmailOptions.SectionPath))
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<ResendEmailOptions>, ResendOptionsValidator>();
        builder.Services.AddSingleton<ResendApiKeyProvider>();
        builder.Services.AddSingleton<ResendRequestMapper>();
        builder.Services.AddSingleton<ResendResponseMapper>();
        builder.Services.AddHttpClient<ResendEmailProvider>((serviceProvider, httpClient) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ResendEmailOptions>>().Value;
            var emailOptions = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
            httpClient.BaseAddress = new Uri(options.ApiBaseUrl, UriKind.Absolute);
            httpClient.Timeout = TimeSpan.FromSeconds(emailOptions.Delivery.TimeoutSeconds);
        });
        builder.Services.AddTransient<IEmailProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<ResendEmailProvider>());

        return builder;
    }

    /// <summary>
    /// Adds the Resend email provider to a Procyon.Email registration.
    /// </summary>
    /// <param name="builder">The Procyon.Email builder.</param>
    /// <returns>The current builder.</returns>
    public static IProcyonEmailBuilder AddResendEmailProvider(this IProcyonEmailBuilder builder)
    {
        return builder.UseResend();
    }
}
