using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;

namespace Procyon.Email.Validation;

internal sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        var failures = new List<string>();

        if (options.Enabled && string.IsNullOrWhiteSpace(options.Provider))
        {
            failures.Add("Procyon:Email:Provider is required when email delivery is enabled.");
        }

        ValidateAddress(options.DefaultSender, "Procyon:Email:DefaultSender", required: options.Enabled, failures);
        ValidateAddress(options.DefaultReplyTo, "Procyon:Email:DefaultReplyTo", required: false, failures);

        if (options.Delivery.TimeoutSeconds <= 0)
        {
            failures.Add("Procyon:Email:Delivery:TimeoutSeconds must be greater than zero.");
        }

        if (options.Delivery.MaximumRecipientsPerMessage <= 0)
        {
            failures.Add("Procyon:Email:Delivery:MaximumRecipientsPerMessage must be greater than zero.");
        }

        if (options.Delivery.MaximumAttachmentSizeMb <= 0)
        {
            failures.Add("Procyon:Email:Delivery:MaximumAttachmentSizeMb must be greater than zero.");
        }

        if (options.Retry.MaximumAttempts <= 0)
        {
            failures.Add("Procyon:Email:Retry:MaximumAttempts must be greater than zero.");
        }

        if (options.Retry.InitialDelaySeconds <= 0)
        {
            failures.Add("Procyon:Email:Retry:InitialDelaySeconds must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateAddress(
        EmailAddressOptions? address,
        string path,
        bool required,
        ICollection<string> failures)
    {
        if (address is null || string.IsNullOrWhiteSpace(address.Address))
        {
            if (required)
            {
                failures.Add($"{path}:Address is required.");
            }

            return;
        }

        try
        {
            _ = new EmailAddress(address.Address, address.Name);
        }
        catch (EmailValidationException exception)
        {
            failures.Add($"{path}:Address is invalid: {exception.Message}");
        }
    }
}
