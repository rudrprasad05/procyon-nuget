using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;
using Procyon.Email.Providers;

namespace Procyon.Email.Validation;

internal sealed class EmailCapabilityValidator(
    IOptionsMonitor<EmailOptions> options,
    ILogger<EmailCapabilityValidator> logger)
{
    public void Validate(EmailMessage message, IEmailProvider provider)
    {
        Check(message.HtmlBody is not null, EmailProviderCapabilities.HtmlBody, "HTML body", provider);
        Check(message.TextBody is not null, EmailProviderCapabilities.TextBody, "plain-text body", provider);
        Check(message.Attachments.Count > 0, EmailProviderCapabilities.Attachments, "attachments", provider);
        Check(message.Attachments.Any(attachment => attachment.IsInline), EmailProviderCapabilities.InlineAttachments, "inline attachments", provider);
        Check(message.Cc.Count > 0, EmailProviderCapabilities.Cc, "CC recipients", provider);
        Check(message.Bcc.Count > 0, EmailProviderCapabilities.Bcc, "BCC recipients", provider);
        Check(message.ReplyTo.Count > 0, EmailProviderCapabilities.ReplyTo, "reply-to recipients", provider);
        Check(message.Headers.Count > 0, EmailProviderCapabilities.CustomHeaders, "custom headers", provider);
        Check(message.Tags.Count > 0, EmailProviderCapabilities.Tags, "tags", provider);
        Check(message.SendAt is not null, EmailProviderCapabilities.ScheduledSending, "scheduled sending", provider);
        Check(!string.IsNullOrWhiteSpace(message.IdempotencyKey), EmailProviderCapabilities.Idempotency, "idempotency", provider);
    }

    private void Check(
        bool requested,
        EmailProviderCapabilities capability,
        string featureName,
        IEmailProvider provider)
    {
        if (!requested || provider.Capabilities.HasFlag(capability))
        {
            return;
        }

        switch (options.CurrentValue.UnsupportedFeatureBehaviour)
        {
            case UnsupportedFeatureBehaviour.Throw:
                throw new EmailUnsupportedFeatureException(featureName, provider.Name);
            case UnsupportedFeatureBehaviour.LogWarning:
                logger.LogWarning(
                    "Email provider {ProviderName} does not support requested feature {FeatureName}.",
                    provider.Name,
                    featureName);
                break;
            case UnsupportedFeatureBehaviour.Ignore:
                break;
            default:
                throw new EmailValidationException($"Unsupported feature behaviour '{options.CurrentValue.UnsupportedFeatureBehaviour}' is not recognized.");
        }
    }
}
