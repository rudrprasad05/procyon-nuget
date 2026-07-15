using Microsoft.Extensions.Options;
using Procyon.Email.Configuration;
using Procyon.Email.Resend.Configuration;

namespace Procyon.Email.Resend.Validation;

internal sealed class ResendOptionsValidator(IOptions<EmailOptions> emailOptions) : IValidateOptions<ResendEmailOptions>
{
    public ValidateOptionsResult Validate(string? name, ResendEmailOptions options)
    {
        if (!string.Equals(emailOptions.Value.Provider, ResendEmailOptions.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ApiBaseUrl) ||
            !Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var apiBaseUri) ||
            apiBaseUri.Scheme is not ("http" or "https"))
        {
            failures.Add("Procyon:Email:Resend:ApiBaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKeyEnvironmentVariable))
        {
            failures.Add("Procyon:Email:Resend:ApiKeyEnvironmentVariable is required.");
        }
        else if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(options.ApiKeyEnvironmentVariable)))
        {
            failures.Add($"Environment variable '{options.ApiKeyEnvironmentVariable}' is required for Resend email provider.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
