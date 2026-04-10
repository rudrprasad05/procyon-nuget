using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Media.Abstractions.Interfaces;

public interface IMediaPathGenerator
{
    string Generate(MediaUploadOptions options);
}