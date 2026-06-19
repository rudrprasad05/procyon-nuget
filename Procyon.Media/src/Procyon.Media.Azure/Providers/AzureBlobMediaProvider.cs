using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Procyon.Media.Abstractions.Interfaces;

namespace Procyon.Media.Azure.Providers;

public class AzureBlobMediaProvider : IMediaProvider
{
    private readonly BlobContainerClient _container;

    public AzureBlobMediaProvider(BlobContainerClient container)
    {
        _container = container;
    }

    public async Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(path);

        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        }, ct);

        return path;
    }

    public async Task DeleteAsync(string path, CancellationToken ct)
    {
        await _container.GetBlobClient(path)
            .DeleteIfExistsAsync(cancellationToken: ct);
    }

    public async Task<Stream> GetAsync(string path, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(path);
        var result = await blob.DownloadStreamingAsync();
        return result.Value.Content;
    }

    public Task<string> GetSignedUrlAsync(string path, TimeSpan expiry, CancellationToken ct)
    {
        var blob = _container.GetBlobClient(path);

        var sas = new BlobSasBuilder
        {
            BlobContainerName = _container.Name,
            BlobName = path,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
        };

        sas.SetPermissions(BlobSasPermissions.Read);

        return Task.FromResult(blob.GenerateSasUri(sas).ToString());
    }
}