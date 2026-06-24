using System.Text.Json;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Options;
using Procyon.Logging.Services;

namespace Procyon.Logging.Tests;

public class QueueAndWriterTests
{
    [Fact]
    public async Task Queue_CanEnqueue_AndWriterWritesJsonLine()
    {
        var root = TestPaths.CreateTempDirectory();
        var queue = new ProcyonLogQueue();
        var timestamp = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var entry = new ProcyonLogEntry
        {
            Timestamp = timestamp,
            Level = ProcyonLogLevel.Information,
            Message = "Something happened",
            Data = new { userId = 42 }
        };

        Assert.True(queue.TryEnqueue(entry));

        var writer = new ProcyonFileLogWriter(
            new TestOptionsMonitor<ProcyonLoggingOptions>(new ProcyonLoggingOptions()),
            new TestHostEnvironment(root));

        await writer.WriteAsync(entry);

        var path = writer.GetLogFilePath(timestamp);
        var line = Assert.Single(await File.ReadAllLinesAsync(path));
        using var document = JsonDocument.Parse(line);

        Assert.Equal("Information", document.RootElement.GetProperty("level").GetString());
        Assert.Equal("Something happened", document.RootElement.GetProperty("message").GetString());
        Assert.Equal(42, document.RootElement.GetProperty("data").GetProperty("userId").GetInt32());
    }
}
