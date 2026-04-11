using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Procyon.Media.Abstractions.Models;

public class MediaUploadResult
{
    public string Key { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string? Hash { get; set; } = null;
    public long Size { get; set; } = 0;
    public string ContentType { get; set; } = "file";
    public bool IsDuplicate { get; set; } = false;
}