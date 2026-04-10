using Microsoft.Extensions.Options;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;
using Procyon.Media.Options;

namespace Procyon.Media.Builders;

public class DefaultMediaPathGenerator : IMediaPathGenerator
{
    private readonly MediaOptions _options;

    public DefaultMediaPathGenerator(IOptions<MediaOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(MediaUploadOptions options)
    {
        var folder = _options.DefaultFolder;

        var fileName = options.FileName ?? "file";

        if (options.GenerateUniqueName)
        {
            var ext = Path.GetExtension(fileName);
            fileName = $"{Guid.NewGuid()}{ext}";
        }

        if (!string.IsNullOrWhiteSpace(folder))
        {
            return $"{folder.TrimEnd('/')}/{fileName}";
        }

        return fileName;
    }
}