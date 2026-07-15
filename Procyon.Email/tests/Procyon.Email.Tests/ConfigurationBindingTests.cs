using Microsoft.Extensions.Configuration;
using Procyon.Email.Abstractions;
using Procyon.Email.Configuration;

namespace Procyon.Email.Tests;

public class ConfigurationBindingTests
{
    [Fact]
    public void EmailOptions_BindFromProcyonEmailSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Enabled"] = "true",
                ["Procyon:Email:Provider"] = "Resend",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:DefaultSender:Name"] = "Example",
                ["Procyon:Email:DefaultReplyTo:Address"] = "support@example.com",
                ["Procyon:Email:Delivery:TimeoutSeconds"] = "45",
                ["Procyon:Email:Delivery:MaximumRecipientsPerMessage"] = "20",
                ["Procyon:Email:Delivery:MaximumAttachmentSizeMb"] = "10",
                ["Procyon:Email:Retry:MaximumAttempts"] = "4",
                ["Procyon:Email:Retry:InitialDelaySeconds"] = "3",
                ["Procyon:Email:UnsupportedFeatureBehaviour"] = "LogWarning"
            })
            .Build();

        var options = configuration.GetSection(EmailOptions.SectionPath).Get<EmailOptions>();

        Assert.NotNull(options);
        Assert.Equal("Resend", options.Provider);
        Assert.Equal("no-reply@example.com", options.DefaultSender?.Address);
        Assert.Equal("Example", options.DefaultSender?.Name);
        Assert.Equal("support@example.com", options.DefaultReplyTo?.Address);
        Assert.Equal(45, options.Delivery.TimeoutSeconds);
        Assert.Equal(20, options.Delivery.MaximumRecipientsPerMessage);
        Assert.Equal(10, options.Delivery.MaximumAttachmentSizeMb);
        Assert.Equal(4, options.Retry.MaximumAttempts);
        Assert.Equal(3, options.Retry.InitialDelaySeconds);
        Assert.Equal(UnsupportedFeatureBehaviour.LogWarning, options.UnsupportedFeatureBehaviour);
    }
}
