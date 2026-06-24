# Procyon NuGet Packages

Reusable .NET 8 libraries for common application infrastructure. The repository is split into focused package families so each concern can be adopted independently.

## Overview

Procyon packages are designed around small abstractions, dependency injection, and provider-based implementations. The main package families currently cover:

- `Procyon.Logging`: lightweight JSON file logging for ASP.NET Core.
- `Procyon.Media`: media upload, retrieval, deletion, URL resolution, hashing, and storage providers.

See each package README for setup and detailed configuration:

- [Procyon.Logging](Procyon.Logging/README.md)
- [Procyon.Media](Procyon.Media/README.md)

## Packages

| Package | Purpose |
| --- | --- |
| `Procyon.Logging.Abstractions` | Logging contracts, log entry models, levels, and `[NoLog]`. |
| `Procyon.Logging` | Request logging middleware, file writer, retention, queueing, and browser log page. |
| `Procyon.Media.Abstractions` | Media service, provider, path, URL, and upload result contracts. |
| `Procyon.Media` | Core media service, hashing, path generation, URL resolution, and DI setup. |
| `Procyon.Media.S3` | AWS S3 storage provider for `Procyon.Media`. |
| `Procyon.Media.Azure` | Azure Blob Storage provider for `Procyon.Media`. |

## Architecture

```text
Procyon.Logging.Abstractions
        ^
Procyon.Logging

Procyon.Media.Abstractions
        ^
Procyon.Media
        ^
Storage providers:
  - Procyon.Media.S3
  - Procyon.Media.Azure
```

## Repository Structure

```text
Procyon.Logging/
  src/
  examples/
  tests/

Procyon.Media/
  src/
  examples/
  tests/
```
