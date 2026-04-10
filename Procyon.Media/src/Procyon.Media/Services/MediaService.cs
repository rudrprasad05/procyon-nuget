using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;

namespace Procyon.Media.Services;

public class MediaService : IMediaService
{
    private readonly IMediaProvider _provider;
    private readonly IMediaPathGenerator _pathGenerator;
    private readonly IMediaUrlResolver _urlResolver;

    public MediaService(
        IMediaProvider provider,
        IMediaPathGenerator pathGenerator,
        IMediaUrlResolver urlResolver)
    {
        _provider = provider;
        _pathGenerator = pathGenerator;
        _urlResolver = urlResolver;
    }

    public async Task<MediaUploadResult> UploadAsync(
        Stream stream,
        MediaUploadOptions options,
        CancellationToken ct = default)
    {
        var path = _pathGenerator.Generate(options);

        await _provider.UploadAsync(
            stream,
            path,
            options.ContentType ?? "application/octet-stream",
            ct);

        return new MediaUploadResult
        {
            Key = path,
            Url = _urlResolver.Resolve(path)
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