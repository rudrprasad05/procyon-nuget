using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Procyon.Logging.Tests;

internal sealed class TestHostEnvironment : IHostEnvironment
{
    public TestHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
        ContentRootFileProvider = new NullFileProvider();
    }

    public string EnvironmentName { get; set; } = Environments.Development;
    public string ApplicationName { get; set; } = "Procyon.Logging.Tests";
    public string ContentRootPath { get; set; }
    public IFileProvider ContentRootFileProvider { get; set; }
}
