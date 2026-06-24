using Microsoft.Extensions.Options;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Options;

namespace Procyon.Logging.Services;

public sealed class ProcyonLogger : IProcyonLogger
{
    private readonly ProcyonLogQueue _queue;
    private readonly IOptionsMonitor<ProcyonLoggingOptions> _options;

    public ProcyonLogger(ProcyonLogQueue queue, IOptionsMonitor<ProcyonLoggingOptions> options)
    {
        _queue = queue;
        _options = options;
    }

    public void Log(ProcyonLogLevel level, string message, object? data = null, Exception? exception = null)
    {
        var options = _options.CurrentValue;

        if (!ShouldLog(options, level))
            return;

        _queue.TryEnqueue(new ProcyonLogEntry
        {
            Timestamp = DateTimeOffset.UtcNow,
            Level = level,
            Message = message,
            Source = "custom",
            Data = data,
            Exception = exception is null ? null : CreateException(exception)
        });
    }

    public void Trace(string message, object? data = null)
        => Log(ProcyonLogLevel.Trace, message, data);

    public void Debug(string message, object? data = null)
        => Log(ProcyonLogLevel.Debug, message, data);

    public void Info(string message, object? data = null)
        => Log(ProcyonLogLevel.Information, message, data);

    public void Information(string message, object? data = null)
        => Log(ProcyonLogLevel.Information, message, data);

    public void Warning(string message, object? data = null)
        => Log(ProcyonLogLevel.Warning, message, data);

    public void Error(Exception exception, string message, object? data = null)
        => Log(ProcyonLogLevel.Error, message, data, exception);

    public void Error(string message, object? data = null)
        => Log(ProcyonLogLevel.Error, message, data);

    public void Critical(Exception exception, string message, object? data = null)
        => Log(ProcyonLogLevel.Critical, message, data, exception);

    public void Critical(string message, object? data = null)
        => Log(ProcyonLogLevel.Critical, message, data);

    internal static bool ShouldLog(ProcyonLoggingOptions options, ProcyonLogLevel level)
        => options.Enabled && level >= options.MinimumLevel;

    internal static ProcyonLogException CreateException(Exception exception)
        => new()
        {
            Type = exception.GetType().FullName ?? exception.GetType().Name,
            Message = exception.Message,
            StackTrace = exception.StackTrace
        };
}
