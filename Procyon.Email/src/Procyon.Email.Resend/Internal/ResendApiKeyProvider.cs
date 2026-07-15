using Microsoft.Extensions.Options;
using Procyon.Email.Resend.Configuration;

namespace Procyon.Email.Resend.Internal;

internal sealed class ResendApiKeyProvider(IOptionsMonitor<ResendEmailOptions> options)
{
    public string GetApiKey()
    {
        var environmentVariable = options.CurrentValue.ApiKeyEnvironmentVariable;
        return Environment.GetEnvironmentVariable(environmentVariable)
            ?? throw new InvalidOperationException($"Environment variable '{environmentVariable}' is required for Resend email provider.");
    }
}
