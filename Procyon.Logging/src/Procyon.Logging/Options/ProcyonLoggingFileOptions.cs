namespace Procyon.Logging.Options;

public sealed class ProcyonLoggingFileOptions
{
    public bool Enabled { get; set; } = true;
    public string Path { get; set; } = "logs";
    public ProcyonLogFileMode Mode { get; set; } = ProcyonLogFileMode.Daily;
    public string SingleFileName { get; set; } = "procyon-log.json";
    public string DateFileFormat { get; set; } = "dd-MM-yy";
    public bool RetentionEnabled { get; set; } = true;
    public int RetainDays { get; set; } = 5;
}
