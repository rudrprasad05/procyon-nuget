using Amazon.S3;
using Amazon.S3.Model;
using Procyon.Media.Abstractions.Interfaces;

namespace Procyon.Media.S3.Providers;

public class S3MediaProvider : IMediaProvider
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public S3MediaProvider(IAmazonS3 s3, string bucket)
    {
        _s3 = s3;
        _bucket = bucket;
    }

    public async Task<string> UploadAsync(
        Stream stream,
        string path,
        string contentType,
        CancellationToken ct)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = path,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(request, ct);
        return path;
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        await _s3.DeleteObjectAsync(_bucket, path, ct);
    }

    public async Task<Stream> GetAsync(string path, CancellationToken ct)
    {
        var response = await _s3.GetObjectAsync(_bucket, path, ct);
        return response.ResponseStream;
    }

    public Task<string> GetSignedUrlAsync(
        string path,
        TimeSpan expiry,
        CancellationToken ct)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = path,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        return Task.FromResult(_s3.GetPreSignedURL(request));
    }
}