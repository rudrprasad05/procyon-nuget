using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Procyon.Media.Abstractions.Models;

public class MediaUploadOptions
{
    public string? FileName { get; set; }
    public string? Folder { get; set; }
    public string? ContentType { get; set; }
    public bool GenerateUniqueName { get; set; } = true;
}