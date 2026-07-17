using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procyon.Email.Abstractions;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;

namespace Procyon.Email.Tests;

public class EmailRetryTests
{
    [Fact]
    public async Task SendAsync_DoesNotRetryRejectedProviderResults()
    {
        var provider = new RejectingEmailProvider();
        var services = new ServiceCollection();
        services.AddProcyonEmail(CreateConfiguration());
        services.AddSingleton<IEmailProvider>(provider);

        var sender = services.BuildServiceProvider().GetRequiredService<IEmailSender>();

        var result = await sender.SendAsync(new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Subject",
            HtmlBody = "<p>Hello</p>",
            IdempotencyKey = "retry-test"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(EmailSendStatus.Rejected, result.Status);
        Assert.Equal(1, result.AttemptCount);
        Assert.Equal(1, provider.AttemptCount);
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Rejecting",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:Retry:Enabled"] = "true",
                ["Procyon:Email:Retry:MaximumAttempts"] = "3",
                ["Procyon:Email:Retry:InitialDelaySeconds"] = "1",
                ["Procyon:Email:Retry:RequireIdempotencyKey"] = "true"
            })
            .Build();
    }

    private sealed class RejectingEmailProvider : IEmailProvider
    {
        public string Name => "Rejecting";

        public EmailProviderCapabilities Capabilities =>
            EmailProviderCapabilities.HtmlBody |
            EmailProviderCapabilities.Idempotency;

        public int AttemptCount { get; private set; }

        public Task<EmailProviderResult> SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            AttemptCount++;
            return Task.FromResult(new EmailProviderResult
            {
                Succeeded = false,
                Status = EmailSendStatus.Rejected,
                ErrorCode = "rejected",
                ErrorMessage = "Rejected."
            });
        }
    }
}
