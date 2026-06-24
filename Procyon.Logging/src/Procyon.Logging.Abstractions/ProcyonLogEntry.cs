namespace Procyon.Logging.Abstractions;

public sealed class ProcyonLogEntry
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ProcyonLogLevel Level { get; set; } = ProcyonLogLevel.Information;
    public string Message { get; set; } = default!;
    public string? Source { get; set; }
    public string? TraceId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public int? StatusCode { get; set; }
    public double? DurationMs { get; set; }
    public IReadOnlyDictionary<string, string[]>? Headers { get; set; }
    public string? RequestBody { get; set; }
    public string? ResponseBody { get; set; }
    public object? Data { get; set; }
    public ProcyonLogException? Exception { get; set; }
}
