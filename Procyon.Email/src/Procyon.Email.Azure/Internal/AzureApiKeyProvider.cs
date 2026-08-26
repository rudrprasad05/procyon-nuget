using Microsoft.Extensions.Options;
using Procyon.Email.Azure.Configuration;

namespace Procyon.Email.Azure.Internal;

internal sealed class AzureApiKeyProvider(IOptionsMonitor<AzureEmailOptions> options)
{
    public string GetConnectionString()
    {
        return options.CurrentValue.ConnectionString
            ?? throw new InvalidOperationException("Procyon:Email:Azure:ConnectionString is required for Azure email provider.");
    }

    public AzureConnectionString GetConnectionDetails()
    {
        return AzureConnectionString.TryParse(GetConnectionString())
            ?? throw new InvalidOperationException("Procyon:Email:Azure:ConnectionString must contain Endpoint and AccessKey values.");
    }
}
