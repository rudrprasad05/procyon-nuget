using Procyon.Logging.Options;
using Procyon.Logging.Services;

namespace Procyon.Logging.Tests;

public class FileNamingTests
{
    [Fact]
    public void GetLogFilePath_UsesDailyDateFormat_ByDefault()
    {
        var root = TestPaths.CreateTempDirectory();
        var writer = CreateWriter(root, new ProcyonLoggingOptions());

        var path = writer.GetLogFilePath(new DateTimeOffset(2026, 6, 19, 1, 2, 3, TimeSpan.Zero));

        Assert.Equal(System.IO.Path.Combine(root, "logs", "procyon-log-19-06-26.json"), path);
    }

    [Fact]
    public void GetLogFilePath_UsesSingleFileName_WhenSingleMode()
    {
        var root = TestPaths.CreateTempDirectory();
        var writer = CreateWriter(root, new ProcyonLoggingOptions
        {
            File =
            {
                Mode = ProcyonLogFileMode.Single,
                SingleFileName = "everything.json"
            }
        });

        var path = writer.GetLogFilePath(DateTimeOffset.UtcNow);

        Assert.Equal(System.IO.Path.Combine(root, "logs", "everything.json"), path);
    }

    private static ProcyonFileLogWriter CreateWriter(string root, ProcyonLoggingOptions options)
        => new(new TestOptionsMonitor<ProcyonLoggingOptions>(options), new TestHostEnvironment(root));
}
