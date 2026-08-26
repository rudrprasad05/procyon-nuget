using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Procyon.Email.Azure.Configuration;
using Procyon.Email.Azure.DependencyInjection;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;

namespace Procyon.Email.Azure.Tests;

public class AzureOptionsTests
{
    [Fact]
    public void AzureEmailOptions_UsesExpectedProviderConstants()
    {
        Assert.Equal("Azure", AzureEmailOptions.ProviderName);
        Assert.Equal("Procyon:Email:Azure", AzureEmailOptions.SectionPath);
    }

    [Fact]
    public void AzureValidation_FailsWhenConnectionStringIsMissing()
    {
        var services = new ServiceCollection();
        services
            .AddProcyonEmail(CreateConfiguration(connectionString: null))
            .UseAzure();

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<AzureEmailOptions>>().Value);
        Assert.Contains("Procyon:Email:Azure:ConnectionString is required.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AzureValidation_FailsWhenSenderEmailIsMissing()
    {
        var services = new ServiceCollection();
        services
            .AddProcyonEmail(CreateConfiguration(senderEmail: null))
            .UseAzure();

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<AzureEmailOptions>>().Value);
        Assert.Contains("Procyon:Email:Azure:SenderEmail is required.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UseAzure_RegistersAzureProviderWhenConfigurationIsValid()
    {
        var services = new ServiceCollection();
        services
            .AddProcyonEmail(CreateConfiguration())
            .UseAzure();

        using var provider = services.BuildServiceProvider();

        var emailProvider = Assert.Single(provider.GetServices<IEmailProvider>());
        Assert.Equal(AzureEmailOptions.ProviderName, emailProvider.Name);
    }

    private static IConfiguration CreateConfiguration(
        string? connectionString = "Endpoint=https://contoso.communication.azure.com/;AccessKey=dGVzdC1rZXk=",
        string? senderEmail = "no-reply@example.com")
    {
        var values = new Dictionary<string, string?>
        {
            ["Procyon:Email:Provider"] = "Azure",
            ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
            ["Procyon:Email:Azure:SenderEmail"] = senderEmail
        };

        if (connectionString is not null)
        {
            values["Procyon:Email:Azure:ConnectionString"] = connectionString;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
