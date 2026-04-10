using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Procyon.Media.Abstractions.Models;

public class MediaUploadResult
{
    public string Key { get; set; } = default!;
    public string Url { get; set; } = default!;
}