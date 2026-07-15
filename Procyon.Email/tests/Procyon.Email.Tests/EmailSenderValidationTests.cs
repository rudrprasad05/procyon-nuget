using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procyon.Email.Abstractions;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;

namespace Procyon.Email.Tests;

public class EmailSenderValidationTests
{
    [Fact]
    public async Task SendAsync_RequiresSubject()
    {
        var sender = CreateSender(EmailProviderCapabilities.HtmlBody);
        var message = ValidMessage() with { Subject = null };

        await Assert.ThrowsAsync<EmailValidationException>(() => sender.SendAsync(message));
    }

    [Fact]
    public async Task SendAsync_RequiresRecipient()
    {
        var sender = CreateSender(EmailProviderCapabilities.HtmlBody);
        var message = ValidMessage() with { To = Array.Empty<EmailAddress>() };

        await Assert.ThrowsAsync<EmailValidationException>(() => sender.SendAsync(message));
    }

    [Fact]
    public async Task SendAsync_RequiresBody()
    {
        var sender = CreateSender(EmailProviderCapabilities.HtmlBody);
        var message = ValidMessage() with { HtmlBody = null };

        await Assert.ThrowsAsync<EmailValidationException>(() => sender.SendAsync(message));
    }

    [Fact]
    public async Task SendAsync_ThrowsForUnsupportedCapabilityByDefault()
    {
        var sender = CreateSender(EmailProviderCapabilities.HtmlBody);
        var message = ValidMessage() with { Bcc = [new EmailAddress("hidden@example.com")] };

        await Assert.ThrowsAsync<EmailUnsupportedFeatureException>(() => sender.SendAsync(message));
    }

    [Fact]
    public async Task SendAsync_IgnoresUnsupportedCapabilityWhenConfigured()
    {
        var sender = CreateSender(
            EmailProviderCapabilities.HtmlBody,
            unsupportedFeatureBehaviour: "Ignore");
        var message = ValidMessage() with { Bcc = [new EmailAddress("hidden@example.com")] };

        var result = await sender.SendAsync(message);

        Assert.True(result.Succeeded);
        Assert.Equal("fake-message-id", result.ProviderMessageId);
    }

    private static IEmailSender CreateSender(
        EmailProviderCapabilities capabilities,
        string unsupportedFeatureBehaviour = "Throw")
    {
        var services = new ServiceCollection();
        services.AddProcyonEmail(CreateConfiguration(unsupportedFeatureBehaviour));
        services.AddSingleton<IEmailProvider>(new FakeEmailProvider(capabilities));

        return services.BuildServiceProvider().GetRequiredService<IEmailSender>();
    }

    private static IConfiguration CreateConfiguration(string unsupportedFeatureBehaviour)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Fake",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:UnsupportedFeatureBehaviour"] = unsupportedFeatureBehaviour
            })
            .Build();
    }

    private static EmailMessage ValidMessage()
    {
        return new EmailMessage
        {
            To = [new EmailAddress("user@example.com")],
            Subject = "Subject",
            HtmlBody = "<p>Hello</p>"
        };
    }

    private sealed class FakeEmailProvider(EmailProviderCapabilities capabilities) : IEmailProvider
    {
        public string Name => "Fake";

        public EmailProviderCapabilities Capabilities { get; } = capabilities;

        public Task<EmailProviderResult> SendAsync(
            EmailMessage message,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailProviderResult.Accepted("fake-message-id"));
        }
    }
}
