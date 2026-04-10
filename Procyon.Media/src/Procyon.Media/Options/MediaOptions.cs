namespace Procyon.Media.Options;

public class MediaOptions
{
    public string Provider { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string? DefaultFolder { get; set; }
    public bool EnableHashing { get; set; } = false;

}