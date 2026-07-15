using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;
using Procyon.Email.Resend.Configuration;
using Procyon.Email.Resend.DependencyInjection;

namespace Procyon.Email.Resend.Tests;

public class ResendOptionsTests
{
    [Fact]
    public void ResendEmailOptions_UsesDefaultEnvironmentVariableName()
    {
        var options = new ResendEmailOptions();

        Assert.Equal("PROCYON_EMAIL_RESEND_API_KEY", options.ApiKeyEnvironmentVariable);
    }

    [Fact]
    public void ResendValidation_FailsWhenConfiguredEnvironmentVariableIsMissing()
    {
        const string environmentVariable = "PROCYON_EMAIL_RESEND_TEST_MISSING";
        Environment.SetEnvironmentVariable(environmentVariable, null);

        var services = new ServiceCollection();
        services
            .AddProcyonEmail(CreateConfiguration(environmentVariable))
            .UseResend();

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<ResendEmailOptions>>().Value);
        Assert.Contains(environmentVariable, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void UseResend_RegistersResendProviderWhenEnvironmentVariableExists()
    {
        const string environmentVariable = "PROCYON_EMAIL_RESEND_TEST_PRESENT";
        Environment.SetEnvironmentVariable(environmentVariable, "placeholder");

        try
        {
            var services = new ServiceCollection();
            services
                .AddProcyonEmail(CreateConfiguration(environmentVariable))
                .UseResend();

            using var provider = services.BuildServiceProvider();

            var emailProvider = Assert.Single(provider.GetServices<IEmailProvider>());
            Assert.Equal(ResendEmailOptions.ProviderName, emailProvider.Name);
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariable, null);
        }
    }

    private static IConfiguration CreateConfiguration(string environmentVariable)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Resend",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com",
                ["Procyon:Email:Resend:ApiBaseUrl"] = "https://api.resend.com",
                ["Procyon:Email:Resend:ApiKeyEnvironmentVariable"] = environmentVariable
            })
            .Build();
    }
}
