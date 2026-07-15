using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;
using Procyon.Email.Providers;

namespace Procyon.Email.Services;

internal sealed class EmailProviderResolver(
    IEnumerable<IEmailProvider> providers,
    IOptionsMonitor<EmailOptions> options)
{
    public IEmailProvider Resolve()
    {
        var providerName = options.CurrentValue.Provider;
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new EmailConfigurationException("Procyon.Email requires a configured provider when email delivery is enabled.");
        }

        var provider = providers.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, providerName, StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            throw new EmailConfigurationException($"Email provider '{providerName}' is not registered.");
        }

        return provider;
    }
}
