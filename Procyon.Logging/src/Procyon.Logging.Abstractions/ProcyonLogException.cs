namespace Procyon.Logging.Abstractions;

public sealed class ProcyonLogException
{
    public string Type { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? StackTrace { get; set; }
}
