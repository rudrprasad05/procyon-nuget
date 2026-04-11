# Procyon.Media

A modular, extensible media upload library for .NET.

## ✨ Features

- Upload, delete, and retrieve media
- Pluggable storage providers (S3 supported)
- Optional file hashing (SHA256)
- Configurable via DI or appsettings
- Designed for extensibility and reuse

---

## 📦 Installation

```bash
dotnet add package Procyon.Media
dotnet add package Procyon.Media.S3
```

---

## 🚀 Quick Start

### 1. Register services

```csharp
builder.Services.AddProcyonMedia(options =>
{
    options.EnableHashing = true;
});

builder.Services.AddS3Provider(builder.Configuration);
```

---

### 2. Use in controller

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
        using var stream = file.OpenReadStream();

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

---

## 📤 Upload Result

```csharp
public class MediaUploadResult
{
    public string Key { get; set; }
    public string Url { get; set; }
    public string FileName { get; set; }
    public string Hash { get; set; }
    public long Size { get; set; }
    public string ContentType { get; set; }
    public bool IsDuplicate { get; set; }
}
```

---

## ⚙️ Configuration

### appsettings.json

```json
{
  "Procyon": {
    "Media": {
      "EnableHashing": true
    }
  }
}
```

---

## ☁️ S3 Configuration

```env
AWS_ACCESS_KEY_ID=your_key
AWS_SECRET_ACCESS_KEY=your_secret

AWS_S3__BucketName=your-bucket
AWS_S3__Region=us-east-1
AWS_S3__FolderName=uploads
```

---

## 🧠 Hashing

When enabled:

- Computes SHA256 hash of uploaded file
- Returned in `MediaUploadResult`
- Can be used for deduplication in your DB

---

## 🧱 Extensibility

You can implement custom providers:

```csharp
public interface IMediaProvider
{
    Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken ct);
    Task DeleteAsync(string key, CancellationToken ct);
}
```

---

## 🔥 Design Goals

- No hard dependency on storage providers
- No forced persistence layer
- Works in any ASP.NET Core project
- Easy to extend and customize

---

## 🚧 Coming Soon

- Signed upload URLs
- Background processing hooks
- Media transformations
- Deduplication helpers

---

## 📄 License

MIT

---

## 🔗 Source

https://github.com/rudrprasad05/nuget
