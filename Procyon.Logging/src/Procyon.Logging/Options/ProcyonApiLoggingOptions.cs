namespace Procyon.Logging.Options;

public sealed class ProcyonApiLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public bool LogHeaders { get; set; } = false;
    public bool LogQueryString { get; set; } = true;
    public int MaxBodyLength { get; set; } = 4096;
}
