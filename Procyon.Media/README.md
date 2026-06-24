# Procyon.Media

A modular media upload library for .NET 8 with provider-based storage.

## Overview

`Procyon.Media` provides a small media service abstraction for upload, retrieval, deletion, signed URLs, URL resolution, path generation, and optional SHA256 hashing. Storage is supplied by provider packages, so the core media package does not depend on a specific cloud provider.

## Packages

| Package | Description |
| --- | --- |
| `Procyon.Media.Abstractions` | `IMediaService`, `IMediaProvider`, path generation, URL resolution, upload options, and result models. |
| `Procyon.Media` | Core service implementation, hashing, default path generation, URL resolver, and DI registration. |
| `Procyon.Media.S3` | AWS S3 provider implementation. |
| `Procyon.Media.Azure` | Azure Blob Storage provider implementation. |

Current package versions are `0.1.0` where package metadata is present.

## Features

- Upload media through `IMediaService`
- Delete media by key
- Retrieve media streams by key
- Generate signed URLs through the selected provider
- Resolve public URLs from configured `BaseUrl`
- Generate unique file names by default
- Store files under an optional default folder
- Compute SHA256 hashes when enabled
- Swap storage providers through `IMediaProvider`

## Installation

Install the core package and one provider package.

```bash
dotnet add package Procyon.Media
dotnet add package Procyon.Media.S3
```

For Azure Blob Storage, use the Azure provider project/package instead of the S3 provider.

## Quick Start

Register the core media services and a storage provider.

```csharp
using Procyon.Media;
using Procyon.Media.S3;

builder.Services.AddProcyonMedia(builder.Configuration);
builder.Services.AddS3Provider(builder.Configuration);
```

For Azure Blob Storage:

```csharp
using Procyon.Media;
using Procyon.Media.Azure;

builder.Services.AddProcyonMedia(builder.Configuration);
builder.Services.AddAzureMediaProvider(builder.Configuration);
```

Use `IMediaService` from application code.

```csharp
[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public UploadController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest("File is required");

        await using var stream = file.OpenReadStream();

        var result = await _mediaService.UploadAsync(
            stream,
            new MediaUploadOptions
            {
                FileName = file.FileName,
                ContentType = file.ContentType
            },
            ct);

        return Ok(result);
    }
}
```

## Configuration

Core configuration is read from `Procyon:Media`.

```json
{
  "Procyon": {
    "Media": {
      "Provider": "S3",
      "EnableHashing": true,
      "DefaultFolder": "uploads",
      "BaseUrl": "https://cdn.example.com",
      "S3": {
        "Bucket": "my-bucket"
      }
    }
  }
}
```

Environment variable equivalent:

```env
Procyon__Media__Provider=S3
Procyon__Media__EnableHashing=true
Procyon__Media__DefaultFolder=uploads
Procyon__Media__BaseUrl=https://cdn.example.com
Procyon__Media__S3__Bucket=my-bucket
```

`BaseUrl` is used by the default URL resolver. `DefaultFolder` is prepended to generated keys when set.

## S3 Provider

`AddS3Provider()` reads the bucket from `Procyon:Media:S3:Bucket` and uses the AWS SDK registration from `AWSSDK.Extensions.NETCore.Setup`.

Common environment variables:

```env
AWS_ACCESS_KEY_ID=your-key
AWS_SECRET_ACCESS_KEY=your-secret
AWS_REGION=us-east-1
Procyon__Media__S3__Bucket=my-bucket
```

## Azure Blob Provider

`AddAzureMediaProvider()` reads the Azure Blob connection string from `ConnectionStrings:AzureBlob` and the container name from `Media:Container`.

```json
{
  "ConnectionStrings": {
    "AzureBlob": "UseDevelopmentStorage=true"
  },
  "Media": {
    "Container": "uploads"
  }
}
```

The provider creates the container if it does not already exist.

## Upload Options

```csharp
public class MediaUploadOptions
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public bool GenerateUniqueName { get; set; } = true;
}
```

When `GenerateUniqueName` is true, the default path generator replaces the file name with a new GUID while keeping the extension.

## Upload Result

```csharp
public class MediaUploadResult
{
    public string Key { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public string? Hash { get; set; }
    public long Size { get; set; }
    public string ContentType { get; set; }
    public bool IsDuplicate { get; set; }
}
```

`Hash` is populated only when `EnableHashing` is true. `IsDuplicate` is currently always false; deduplication can be handled by the consuming application using the returned hash.

## Service API

```csharp
public interface IMediaService
{
    Task<MediaUploadResult> UploadAsync(
        Stream stream,
        MediaUploadOptions options,
        CancellationToken ct = default);

    Task DeleteAsync(string key, CancellationToken ct = default);

    Task<Stream> GetAsync(string key, CancellationToken ct = default);

    Task<string> GetSignedUrlAsync(
        string key,
        TimeSpan expiry,
        CancellationToken ct = default);
}
```

## Custom Providers

Implement `IMediaProvider` to add another storage backend.

```csharp
public interface IMediaProvider
{
    Task<string> UploadAsync(
        Stream stream,
        string path,
        string contentType,
        CancellationToken ct);

    Task DeleteAsync(string path, CancellationToken ct);

    Task<Stream> GetAsync(string path, CancellationToken ct);

    Task<string> GetSignedUrlAsync(
        string path,
        TimeSpan expiry,
        CancellationToken ct);
}
```

Then register the implementation as `IMediaProvider` after `AddProcyonMedia()`.

## Example Project

The runnable example is in `examples/Procyon.Example`.

```bash
dotnet run --project Procyon.Media/examples/Procyon.Example/Procyon.Example.csproj
```

The example demonstrates:

- S3-backed uploads
- SQLite persistence for uploaded file metadata
- `POST /api/upload`
- `GET /api/upload`
- `GET /api/upload/signed-url`
- `DELETE /api/upload?key=...`

## Roadmap

- Provider package metadata alignment
- Local disk provider
- Upload validation helpers
- Optional deduplication helpers
- Background processing hooks
- Media transformations
