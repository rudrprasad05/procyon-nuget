using Microsoft.Extensions.Options;
using Procyon.Email.Azure.Configuration;
using Procyon.Email.Configuration;

namespace Procyon.Email.Azure.Validation;

internal sealed class AzureEmailOptionsValidator(IOptions<EmailOptions> emailOptions) : IValidateOptions<AzureEmailOptions>
{
    public ValidateOptionsResult Validate(string? name, AzureEmailOptions options)
    {
        if (!string.Equals(emailOptions.Value.Provider, AzureEmailOptions.ProviderName, StringComparison.OrdinalIgnoreCase))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            failures.Add("Procyon:Email:Azure:ConnectionString is required.");
        }
        else
        {
            var parsed = AzureConnectionString.TryParse(options.ConnectionString);
            if (parsed is null)
            {
                failures.Add("Procyon:Email:Azure:ConnectionString must contain Endpoint and AccessKey values.");
            }
        }

        if (string.IsNullOrWhiteSpace(options.SenderEmail))
        {
            failures.Add("Procyon:Email:Azure:SenderEmail is required.");
        }

        if (!string.IsNullOrWhiteSpace(options.ApiBaseUrl) &&
            (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var apiBaseUri) ||
             apiBaseUri.Scheme is not ("http" or "https")))
        {
            failures.Add("Procyon:Email:Azure:ApiBaseUrl must be an absolute HTTP or HTTPS URL.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
