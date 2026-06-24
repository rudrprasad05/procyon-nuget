using Procyon.Logging.Options;
using Procyon.Logging.Services;

namespace Procyon.Logging.Tests;

public class RetentionTests
{
    [Fact]
    public async Task ApplyRetentionAsync_DeletesJsonFilesOlderThanRetentionWindow()
    {
        var root = TestPaths.CreateTempDirectory();
        var logDirectory = System.IO.Path.Combine(root, "logs");
        Directory.CreateDirectory(logDirectory);

        var oldFile = System.IO.Path.Combine(logDirectory, "old.json");
        var currentFile = System.IO.Path.Combine(logDirectory, "current.json");
        await File.WriteAllTextAsync(oldFile, "{}");
        await File.WriteAllTextAsync(currentFile, "{}");
        File.SetLastWriteTimeUtc(oldFile, DateTime.UtcNow.AddDays(-10));
        File.SetLastWriteTimeUtc(currentFile, DateTime.UtcNow);

        var options = new TestOptionsMonitor<ProcyonLoggingOptions>(new ProcyonLoggingOptions
        {
            File =
            {
                RetentionEnabled = true,
                RetainDays = 5
            }
        });

        var writer = new ProcyonFileLogWriter(options, new TestHostEnvironment(root));
        var retention = new ProcyonLogRetentionService(writer, options);

        await retention.ApplyRetentionAsync();

        Assert.False(File.Exists(oldFile));
        Assert.True(File.Exists(currentFile));
    }
}
