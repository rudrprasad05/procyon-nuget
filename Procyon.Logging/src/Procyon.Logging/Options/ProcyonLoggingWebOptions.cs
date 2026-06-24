namespace Procyon.Logging.Options;

public sealed class ProcyonLoggingWebOptions
{
    public bool Enabled { get; set; } = true;
    public bool DevOnly { get; set; } = true;
    public string Path { get; set; } = "/procyon/logs";
    public bool UseSignalR { get; set; } = true;
    public int FallbackPollingSeconds { get; set; } = 3;
}
