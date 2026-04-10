namespace Procyon.Example.Data;

public class MediaFile
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}