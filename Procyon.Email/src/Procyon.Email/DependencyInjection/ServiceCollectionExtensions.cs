using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Builders;
using Procyon.Email.Configuration;
using Procyon.Email.Providers;
using Procyon.Email.Services;
using Procyon.Email.Validation;

namespace Procyon.Email.DependencyInjection;

/// <summary>
/// Registers provider-independent Procyon email services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Procyon.Email core services and configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A builder that provider packages can extend.</returns>
    public static IProcyonEmailBuilder AddProcyonEmail(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionPath))
            .ValidateOnStart();

        services.AddLogging();
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();
        services.AddSingleton<EmailDefaultsApplicator>();
        services.AddSingleton<EmailMessageValidator>();
        services.AddSingleton<EmailCapabilityValidator>();
        services.AddSingleton<EmailProviderResolver>();
        services.AddSingleton<EmailRetryCoordinator>();
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<IEmailMessageBuilder, EmailMessageBuilder>();

        return new ProcyonEmailBuilder(services, configuration);
    }
}
