using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Builders;
using Procyon.Email.Configuration;
using Procyon.Email.Validation;

namespace Procyon.Email.Services;

internal sealed class EmailSender(
    EmailDefaultsApplicator defaultsApplicator,
    EmailMessageValidator messageValidator,
    EmailCapabilityValidator capabilityValidator,
    EmailProviderResolver providerResolver,
    EmailRetryCoordinator retryCoordinator,
    IOptionsMonitor<EmailOptions> options,
    ILogger<EmailSender> logger) : IEmailSender
{
    public IEmailMessageBuilder Create()
    {
        return new EmailMessageBuilder();
    }

    public async Task<EmailSendResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!options.CurrentValue.Enabled)
        {
            logger.LogInformation("Email delivery is disabled.");
            return EmailSendResult.Failed(null, EmailSendStatus.Disabled, "disabled", "Email delivery is disabled.");
        }

        var provider = providerResolver.Resolve();
        var prepared = defaultsApplicator.Apply(message);

        messageValidator.Validate(prepared);
        capabilityValidator.Validate(prepared, provider);

        logger.LogInformation(
            "Sending email with provider {ProviderName} to {RecipientCount} recipients.",
            provider.Name,
            prepared.To.Count + prepared.Cc.Count + prepared.Bcc.Count);

        var started = TimeProvider.System.GetTimestamp();
        var (providerResult, attemptCount) = await retryCoordinator
            .SendAsync(provider, prepared, cancellationToken)
            .ConfigureAwait(false);
        var elapsed = TimeProvider.System.GetElapsedTime(started);

        logger.LogInformation(
            "Email send completed with provider {ProviderName}, status {Status}, attempts {AttemptCount}, duration {DurationMs}ms, provider message id {ProviderMessageId}.",
            provider.Name,
            providerResult.Status,
            attemptCount,
            elapsed.TotalMilliseconds,
            providerResult.ProviderMessageId);

        return providerResult.Succeeded
            ? EmailSendResult.Accepted(provider.Name, providerResult.ProviderMessageId, attemptCount)
            : EmailSendResult.Failed(
                provider.Name,
                providerResult.Status,
                providerResult.ErrorCode,
                providerResult.ErrorMessage,
                attemptCount);
    }
}
