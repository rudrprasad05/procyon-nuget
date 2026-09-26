# Procyon.Example.Azure

This is the Azure Blob Storage counterpart to `Procyon.Example`. It uses the
same controller, SQLite/EF migration, Swagger, and DotNetEnv startup pattern;
the storage package and provider registration are Azure-specific. The example
references the public `Procyon.Media` and `Procyon.Media.Azure` NuGet packages.

## Setup

From the repository root:

```bash
cd Procyon.Media/examples/Procyon.Example.Azure
cp .env.example .env
```

`DotNetEnv.Env.Load()` reads `.env` from the current directory before
`WebApplication.CreateBuilder`. The defaults in `appsettings.json` use
Azurite; start Azurite before uploading. In `.env`, set
`ConnectionStrings__AzureBlob` to an Azure Storage account connection string
for a real account, `Media__Container` to its container name, and
`Procyon__Media__BaseUrl` to the container's URL. Never commit `.env`.
Signed URLs require credentials able to sign an account SAS.

## Run

```bash
dotnet run
```

The HTTPS profile listens on `https://localhost:7081` and the HTTP profile
on `http://localhost:5220`; browse `/swagger`. If the local ASP.NET Core
development certificate is not trusted, use `curl -k` for HTTPS (or run the
`http` profile). SQLite migrations are applied at startup, as in the S3
example. The `media.db` file is ignored by Git.

## Test

```bash
curl -k -X POST "https://localhost:7081/api/upload" \
  -F "file=@/path/to/image.jpg"
```

The response includes a `key`; substitute it below. `GET /api/upload`
returns the saved metadata records, not blob content.

```bash
curl -k "https://localhost:7081/api/upload"
curl -k "https://localhost:7081/api/upload/file?key=<returned-key>" -o image.jpg
curl -k "https://localhost:7081/api/upload/signed-url?key=<returned-key>&expiresInSeconds=900"
curl -k "https://localhost:7081/api/upload/public-url?key=<returned-key>"
curl -k -X DELETE "https://localhost:7081/api/upload?key=<returned-key>"
```

The first four controller operations (upload, metadata list, signed URL, and
delete) mirror the S3 example's routes and response shapes. The `file` and
`public-url` GET routes additionally demonstrate the existing provider-neutral
`IMediaService.GetAsync` and `IMediaUrlResolver.Resolve` APIs. A resolved
public URL is not necessarily readable if the container is private.
