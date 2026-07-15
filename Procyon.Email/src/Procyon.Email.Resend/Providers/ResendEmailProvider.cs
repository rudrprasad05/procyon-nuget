using Microsoft.Extensions.Options;
using Procyon.Email.Abstractions;
using Procyon.Email.Providers;
using Procyon.Email.Resend.Configuration;
using Procyon.Email.Resend.Mapping;

namespace Procyon.Email.Resend.Providers;

internal sealed class ResendEmailProvider(
    HttpClient httpClient,
    IOptionsMonitor<ResendEmailOptions> options,
    ResendRequestMapper requestMapper,
    ResendResponseMapper responseMapper) : IEmailProvider
{
    public string Name => ResendEmailOptions.ProviderName;

    public EmailProviderCapabilities Capabilities =>
        EmailProviderCapabilities.HtmlBody |
        EmailProviderCapabilities.TextBody |
        EmailProviderCapabilities.Attachments |
        EmailProviderCapabilities.InlineAttachments |
        EmailProviderCapabilities.Cc |
        EmailProviderCapabilities.Bcc |
        EmailProviderCapabilities.ReplyTo |
        EmailProviderCapabilities.CustomHeaders |
        EmailProviderCapabilities.Tags |
        EmailProviderCapabilities.ScheduledSending |
        EmailProviderCapabilities.Idempotency;

    public Task<EmailProviderResult> SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        _ = httpClient;
        _ = options.CurrentValue;
        _ = requestMapper.Map(message);
        _ = responseMapper;
        _ = cancellationToken;

        throw new NotImplementedException("Resend HTTP email delivery is scaffolded but not implemented in this architecture task.");
    }
}
