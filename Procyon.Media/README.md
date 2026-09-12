# Procyon.Media

`Procyon.Media` is a small, provider-based media library for .NET 8. The core
package handles storage keys, uploads, retrieval, deletion, URL resolution, and
optional SHA256 hashing. `Procyon.Media.Azure` supplies Azure Blob Storage.

## Packages

| Package | Purpose |
| --- | --- |
| `Procyon.Media.Abstractions` | Public service, provider, path, URL, options, and result contracts. |
| `Procyon.Media` | Core implementation and dependency-injection registration. |
| `Procyon.Media.Azure` | Azure Blob Storage provider and registration. |
| `Procyon.Media.S3` | Optional AWS S3 provider; not part of the Azure release. |

## Azure quick start

### Install

```bash
dotnet add package Procyon.Media
dotnet add package Procyon.Media.Azure
```

The abstraction package is restored transitively; applications do not need to
install it separately.

### Configure

Core settings live under `Procyon:Media`. The Azure provider reads its
connection string from `ConnectionStrings:AzureBlob` and its container from
`Media:Container`.

```json
{
  "ConnectionStrings": {
    "AzureBlob": "UseDevelopmentStorage=true"
  },
  "Media": {
    "Container": "media"
  },
  "Procyon": {
    "Media": {
      "Provider": "Azure",
      "EnableHashing": true,
      "DefaultFolder": "uploads",
      "BaseUrl": "http://127.0.0.1:10000/devstoreaccount1/media"
    }
  }
}
```

`UseDevelopmentStorage=true` connects to Azurite. For an Azure Storage account,
keep the connection string out of source control. For example, in an ASP.NET
Core application:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AzureBlob" "<connection-string>"
```

Environment variables use ASP.NET Core's double-underscore convention:

```env
ConnectionStrings__AzureBlob=<connection-string>
Media__Container=media
Procyon__Media__Provider=Azure
Procyon__Media__EnableHashing=true
Procyon__Media__DefaultFolder=uploads
Procyon__Media__BaseUrl=https://account.blob.core.windows.net/media
```

The provider creates the configured container when it is first resolved. SAS
URL generation requires credentials capable of signing a SAS, such as a storage
account connection string.

### Register

```csharp
using Procyon.Media;
using Procyon.Media.Azure;

builder.Services.AddProcyonMedia(builder.Configuration);
builder.Services.AddAzureMediaProvider(builder.Configuration);
```

Register one `IMediaProvider` after the core services.

### Upload

```csharp
using Procyon.Media.Abstractions.Interfaces;
using Procyon.Media.Abstractions.Models;

await using var stream = File.OpenRead("photo.png");

var result = await mediaService.UploadAsync(
    stream,
    new MediaUploadOptions
    {
        FileName = "photo.png",
        ContentType = "image/png"
    },
    cancellationToken);

var storageKey = result.Key;
var publicUrl = result.Url;
```

Treat `result.Key`, such as `uploads/0d5...png`, as the canonical storage
identity. `result.Url` is a separately resolved public URL and may not be
accessible if the container is private.

When `GenerateUniqueName` is `true` (the default), the path generator replaces
the original filename with a GUID while preserving its extension. It prepends
`DefaultFolder` when configured. `Hash` is populated only when `EnableHashing`
is enabled. `IsDuplicate` is currently always `false`; deduplication is left to
the consuming application.

### Download or read

```csharp
await using var stream = await mediaService.GetAsync(storageKey, cancellationToken);
// Copy or return the stream to the caller.
```

### Delete

```csharp
await mediaService.DeleteAsync(storageKey, cancellationToken);
```

### URLs

Generate a temporary read-only Azure SAS URL through `IMediaService`:

```csharp
var signedUrl = await mediaService.GetSignedUrlAsync(
    storageKey,
    TimeSpan.FromMinutes(15),
    cancellationToken);
```

Resolve the configured public base URL through `IMediaUrlResolver`:

```csharp
var publicUrl = urlResolver.Resolve(storageKey);
```

## Public API

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

Storage providers implement `IMediaProvider`; the provider-neutral core does
not depend on an Azure or AWS SDK.

## Azure example

The runnable example at
[`examples/Procyon.Media.Example.Azure`](examples/Procyon.Media.Example.Azure)
is a deliberately small ASP.NET Core API using only public package APIs. It
provides:

- `POST /media`
- `GET /media?key=...`
- `DELETE /media?key=...`
- `GET /media/url?key=...&expiresInMinutes=15`
- `GET /media/public-url?key=...`

Run it against Azurite with:

```bash
dotnet run --project Procyon.Media/examples/Procyon.Media.Example.Azure/Procyon.Media.Example.Azure.csproj
```
