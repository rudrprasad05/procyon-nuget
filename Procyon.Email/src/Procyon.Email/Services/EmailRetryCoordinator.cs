using Microsoft.Extensions.Options;
using Procyon.Email.Configuration;
using Procyon.Email.Providers;

namespace Procyon.Email.Services;

internal sealed class EmailRetryCoordinator(IOptionsMonitor<EmailOptions> options)
{
    public async Task<(EmailProviderResult Result, int AttemptCount)> SendAsync(
        IEmailProvider provider,
        Procyon.Email.Abstractions.EmailMessage message,
        CancellationToken cancellationToken)
    {
        var retry = options.CurrentValue.Retry;
        var maximumAttempts = retry.Enabled ? Math.Max(1, retry.MaximumAttempts) : 1;

        if (retry.RequireIdempotencyKey && string.IsNullOrWhiteSpace(message.IdempotencyKey))
        {
            maximumAttempts = 1;
        }

        for (var attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            var result = await provider.SendAsync(message, cancellationToken).ConfigureAwait(false);
            if (result.Succeeded || attempt == maximumAttempts)
            {
                return (result, attempt);
            }

            var delay = retry.UseExponentialBackoff
                ? TimeSpan.FromSeconds(retry.InitialDelaySeconds * Math.Pow(2, attempt - 1))
                : TimeSpan.FromSeconds(retry.InitialDelaySeconds);

            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("Email retry coordination reached an invalid state.");
    }
}
