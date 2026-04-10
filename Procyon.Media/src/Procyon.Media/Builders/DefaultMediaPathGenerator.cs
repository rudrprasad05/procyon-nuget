using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Media.Builders;

public class DefaultMediaPathGenerator : IMediaPathGenerator
{
    public string Generate(MediaUploadOptions options)
    {
        var fileName = options.FileName ?? "file";

        if (options.GenerateUniqueName)
        {
            var ext = Path.GetExtension(fileName);
            fileName = $"{Guid.NewGuid()}{ext}";
        }

        if (!string.IsNullOrWhiteSpace(options.Folder))
        {
            return $"{options.Folder.TrimEnd('/')}/{fileName}";
        }

        return fileName;
    }
}