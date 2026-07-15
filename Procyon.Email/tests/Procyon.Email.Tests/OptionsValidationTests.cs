using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Procyon.Email.Configuration;
using Procyon.Email.DependencyInjection;

namespace Procyon.Email.Tests;

public class OptionsValidationTests
{
    [Fact]
    public void EmailOptions_RequireSelectedProviderWhenEnabled()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Procyon:Email:Enabled"] = "true",
                ["Procyon:Email:DefaultSender:Address"] = "no-reply@example.com"
            })
            .Build();

        services.AddProcyonEmail(configuration);

        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<EmailOptions>>().Value);
        Assert.Contains("Procyon:Email:Provider", exception.Message, StringComparison.Ordinal);
    }
}
