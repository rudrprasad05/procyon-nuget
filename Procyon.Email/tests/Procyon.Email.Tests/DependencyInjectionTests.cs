using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Procyon.Email.Abstractions;
using Procyon.Email.DependencyInjection;
using Procyon.Email.Providers;

namespace Procyon.Email.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddProcyonEmail_RegistersCoreSender()
    {
        var services = new ServiceCollection();

        services.AddProcyonEmail(CreateConfiguration());

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IEmailSender>());
    }

    [Fact]
    public void AddProcyonEmail_DoesNotRegisterProvider()
    {
        var services = new ServiceCollection();

        services.AddProcyonEmail(CreateConfiguration());

        using var provider = services.BuildServiceProvider();

        Assert.Empty(provider.GetServices<IEmailProvider>());
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Provider"] = "Fake",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com"
            })
            .Build();
    }
}
