namespace Procyon.Logging.Abstractions;

public interface IProcyonLogger
{
    void Log(ProcyonLogLevel level, string message, object? data = null, Exception? exception = null);

    void Trace(string message, object? data = null);

    void Debug(string message, object? data = null);

    void Info(string message, object? data = null);

    void Information(string message, object? data = null);

    void Warning(string message, object? data = null);

    void Error(Exception exception, string message, object? data = null);

    void Error(string message, object? data = null);

    void Critical(Exception exception, string message, object? data = null);

    void Critical(string message, object? data = null);
}
