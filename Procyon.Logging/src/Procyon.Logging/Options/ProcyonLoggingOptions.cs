using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Options;

public sealed class ProcyonLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public ProcyonLogLevel MinimumLevel { get; set; } = ProcyonLogLevel.Information;
    public ProcyonLoggingFileOptions File { get; set; } = new();
    public ProcyonLoggingWebOptions Web { get; set; } = new();
    public ProcyonApiLoggingOptions ApiLogging { get; set; } = new();
}
