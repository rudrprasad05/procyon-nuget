using Microsoft.Extensions.Options;
using Procyon.Logging.Options;

namespace Procyon.Logging.Services;

public sealed class ProcyonLogRetentionService
{
    private readonly ProcyonFileLogWriter _fileLogWriter;
    private readonly IOptionsMonitor<ProcyonLoggingOptions> _options;

    public ProcyonLogRetentionService(
        ProcyonFileLogWriter fileLogWriter,
        IOptionsMonitor<ProcyonLoggingOptions> options)
    {
        _fileLogWriter = fileLogWriter;
        _options = options;
    }

    public Task ApplyRetentionAsync(CancellationToken ct = default)
    {
        var options = _options.CurrentValue;

        if (!options.Enabled || !options.File.Enabled || !options.File.RetentionEnabled)
            return Task.CompletedTask;

        if (options.File.RetainDays < 1)
            return Task.CompletedTask;

        var directory = _fileLogWriter.ResolveLogDirectory(options.File.Path);

        if (!Directory.Exists(directory))
            return Task.CompletedTask;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-options.File.RetainDays);

        foreach (var file in Directory.EnumerateFiles(directory, "*.json"))
        {
            ct.ThrowIfCancellationRequested();

            var lastWrite = File.GetLastWriteTimeUtc(file);

            if (lastWrite < cutoff.UtcDateTime)
                File.Delete(file);
        }

        return Task.CompletedTask;
    }
}
