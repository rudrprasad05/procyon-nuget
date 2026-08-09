namespace Procyon.Logging.Options;

public sealed class ProcyonLoggingWebOptions
{
    public bool Enabled { get; set; } = true;
    public bool DevOnly { get; set; } = true;
    public string Path { get; set; } = ProcyonLoggingDefaults.WebPath;
    public bool LogRequests { get; set; } = false;
    public bool UseSignalR { get; set; } = true;
    public int FallbackPollingSeconds { get; set; } = 3;
    public string FaviconPath { get; set; } = ProcyonLoggingDefaults.WebFaviconPath;
}
