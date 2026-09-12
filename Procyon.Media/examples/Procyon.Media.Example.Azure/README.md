# Procyon.Media Azure example

This minimal ASP.NET Core API demonstrates the public `Procyon.Media` and
`Procyon.Media.Azure` APIs for upload, download, deletion, signed URLs, and
public URL resolution.

The checked-in configuration targets Azurite. Start Azurite, then run:

```bash
dotnet run --project Procyon.Media/examples/Procyon.Media.Example.Azure/Procyon.Media.Example.Azure.csproj
```

To use an Azure Storage account instead, store its connection string outside
source control:

```bash
dotnet user-secrets set "ConnectionStrings:AzureBlob" "<connection-string>" \
  --project Procyon.Media/examples/Procyon.Media.Example.Azure/Procyon.Media.Example.Azure.csproj
```

Set `Procyon:Media:BaseUrl` to the container's public base URL if public URL
resolution is required. The signed URL endpoint requires credentials that can
generate an account SAS, such as a storage account connection string.
