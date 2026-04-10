using Microsoft.Extensions.Options;
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;
using Procyon.Media.Options;

namespace Procyon.Media.Services;

public class MediaService : IMediaService
{
    private readonly IMediaProvider _provider;
    private readonly IMediaPathGenerator _pathGenerator;
    private readonly IMediaUrlResolver _urlResolver;
    private readonly IOptions<MediaOptions> _options;

    public MediaService(
        IMediaProvider provider,
        IMediaPathGenerator pathGenerator,
        IMediaUrlResolver urlResolver,
        IOptions<MediaOptions> options)
    {
        _provider = provider;
        _pathGenerator = pathGenerator;
        _urlResolver = urlResolver;
        _options = options;
    }

    public async Task<MediaUploadResult> UploadAsync(
        Stream stream,
        MediaUploadOptions options,
        CancellationToken ct = default)
    {
        string? hash = null;

        if (_options.Value.EnableHashing)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            hash = await FileHashing.ComputeSha256Async(stream);

            if (stream.CanSeek)
                stream.Position = 0;
        }

        var path = _pathGenerator.Generate(options);

        await _provider.UploadAsync(
            stream,
            path,
            options.ContentType ?? "application/octet-stream",
            ct);

        return new MediaUploadResult
        {
            Key = path,
            Url = _urlResolver.Resolve(path),

            FileName = options.FileName ?? Path.GetFileName(path),
            Hash = hash,

            Size = stream.CanSeek ? stream.Length : 0,
            ContentType = options.ContentType ?? "application/octet-stream",

            IsDuplicate = false
        };
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
        => _provider.DeleteAsync(key, ct);

    public Task<Stream> GetAsync(string key, CancellationToken ct = default)
        => _provider.GetAsync(key, ct);

    public Task<string> GetSignedUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default)
        => _provider.GetSignedUrlAsync(key, expiry, ct);
}