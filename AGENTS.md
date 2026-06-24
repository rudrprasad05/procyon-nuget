# AGENTS.md

Guidance for AI agents working in this repository.

## Project Summary

This repository contains reusable .NET 8 NuGet package families under the Procyon name. Packages are intentionally small, config-driven, and organized around dependency injection, abstractions, and provider implementations.

- `Procyon.Logging`: JSON file logging for ASP.NET Core, API request logging middleware, retention cleanup, an optional browser logs page, and SignalR live updates.
- `Procyon.Media`: media upload/retrieval/deletion abstractions, path generation, URL resolution, hashing, and provider-backed storage.
- `Procyon.Media.S3`: AWS S3 storage provider.
- `Procyon.Media.Azure`: Azure Blob Storage provider.

The repository root has documentation and this agent guide. Each package family has its own solution, source, examples, tests, and `Directory.Build.props`.

## Repository Layout

```text
Procyon.Logging/
  Procyon.Logging.sln
  Directory.Build.props
  README.md
  appsettings.example.json
  src/
    Procyon.Logging.Abstractions/
    Procyon.Logging/
  examples/
    Procyon.Logging.Example/
  tests/
    Procyon.Logging.Tests/

Procyon.Media/
  Procyon.Media.sln
  Directory.Build.props
  README.md
  src/
    Procyon.Media.Abstractions/
    Procyon.Media/
    Procyon.Media.S3/
    Procyon.Media.Azure/
  examples/
    Procyon.Example/
  tests/
    Procyon.Media.Tests/
```

There is no root solution currently. Build and test package families through their individual `.sln` files.

## Build, Test, and Run Commands

Use commands from the repository root unless a task requires a package directory.

```bash
dotnet build Procyon.Logging/Procyon.Logging.sln
dotnet test Procyon.Logging/Procyon.Logging.sln
dotnet run --project Procyon.Logging/examples/Procyon.Logging.Example/Procyon.Logging.Example.csproj
```

```bash
dotnet build Procyon.Media/Procyon.Media.sln
dotnet test Procyon.Media/Procyon.Media.sln
dotnet run --project Procyon.Media/examples/Procyon.Example/Procyon.Example.csproj
```

Do not pass secrets or environment variables inline to build, test, or run commands. Use the shell/session environment, user secrets, `.env` files for examples that explicitly load them, or normal ASP.NET Core configuration providers.

## Git Permissions and Versioning

Agents may run `git status`, `git diff`, `git add`, and `git commit` after making changes. Commits should be focused and named clearly, for example:

```bash
git add AGENTS.md
git commit -m "Add repository agent guidance"
```

Before committing, inspect `git status --short` and stage only files that belong to the completed task. Never stage or revert unrelated user changes.

Agents may create Git tags only when the user explicitly asks for a version tag. Do not infer a tag request from a package version change, release note, commit, or build command.

## Worktree Safety

This repository may have uncommitted user changes. Treat all existing modifications as user-owned unless you made them in the current task.

- Do not run destructive commands such as `git reset --hard`, broad checkout/revert commands, or file deletion unless explicitly requested.
- If unrelated files are dirty, ignore them.
- If task-related files are dirty, read them carefully and preserve the user's work while making the requested change.
- Keep edits scoped to the package, example, test, or docs surface involved in the task.

## .NET and Project Conventions

The codebase targets `.NET 8` with nullable reference types and implicit usings enabled. This is normally configured in each package family's `Directory.Build.props`.

Follow these conventions:

- Use file-scoped namespaces.
- Prefer constructor injection and DI extension methods.
- Keep public APIs small and package-oriented.
- Put contracts and shared models in `*.Abstractions` packages.
- Put concrete services, middleware, providers, and registration methods in implementation packages.
- Use `IOptions<T>` or `IOptionsMonitor<T>` for configuration.
- Prefer `CancellationToken ct = default` on async public APIs.
- Use `Task`/`Task<T>` for async APIs.
- Use `sealed` for classes that are not intended for inheritance, especially options, middleware, controllers, and services.
- Preserve existing naming patterns such as `AddProcyonLogging`, `UseProcyonLogging`, `AddProcyonMedia`, `AddS3Provider`, and `AddAzureMediaProvider`.
- Keep comments sparse and useful. Do not add comments that restate obvious code.

## Configuration and Secrets

Configuration uses ASP.NET Core binding conventions.

- Logging config lives under `Procyon:Logging`.
- Media config lives under `Procyon:Media`.
- S3 bucket config is currently read from `Procyon:Media:S3:Bucket`.
- Azure Blob connection strings use `ConnectionStrings:AzureBlob`.
- Azure container config is currently read from `Media:Container`.

Use environment variables where possible for secrets and deployment-specific values. Use double underscores for nested configuration keys:

```env
Procyon__Media__Provider=S3
Procyon__Media__BaseUrl=https://cdn.example.com
Procyon__Media__S3__Bucket=my-bucket
AWS_ACCESS_KEY_ID=your-key
AWS_SECRET_ACCESS_KEY=your-secret
AWS_REGION=us-east-1
ConnectionStrings__AzureBlob=UseDevelopmentStorage=true
```

Do not commit real credentials, production connection strings, access keys, tokens, or private bucket/container names. Keep examples generic or obviously non-secret. If an example needs local values, prefer `.env.example`, `appsettings.example.json`, or documentation.

Do not write commands like this in docs or scripts:

```bash
AWS_ACCESS_KEY_ID=... dotnet run --project ...
```

Instead, instruct users to set environment variables in their shell/session or use the relevant local configuration mechanism before running the standard command.

## Logging Package Patterns

`Procyon.Logging.Abstractions` contains public contracts and models:

- `IProcyonLogger`
- `ProcyonLogEntry`
- `ProcyonLogLevel`
- `ProcyonLogException`
- `[NoLog]`

`Procyon.Logging` contains:

- `AddProcyonLogging(IConfiguration)`
- `UseProcyonLogging()`
- `ProcyonLoggingMiddleware`
- queue, writer, store, retention, and broadcaster services
- browser logs endpoints and SignalR hub

Important behavior:

- `UseProcyonLogging()` adds middleware and maps the optional web log endpoints when endpoint routing is available.
- API request logging can include query strings, headers, request bodies, and response bodies based on config.
- `[NoLog]` skips automatic API request logging for the endpoint only. Custom `IProcyonLogger` calls still work.
- Logs are newline-delimited JSON, one `ProcyonLogEntry` per line.
- The default log folder is `logs`.
- The default daily filename is `procyon-log-dd-MM-yy.json`.
- The default web UI path is `/procyon/logs`.

When changing logging behavior, update the README, `appsettings.example.json`, the runnable example, and tests when relevant.

## Media Package Patterns

`Procyon.Media.Abstractions` contains public contracts and models:

- `IMediaService`
- `IMediaProvider`
- `IMediaPathGenerator`
- `IMediaUrlResolver`
- `MediaUploadOptions`
- `MediaUploadResult`

`Procyon.Media` contains:

- `AddProcyonMedia(IConfiguration)`
- `MediaService`
- `DefaultMediaPathGenerator`
- `DefaultMediaUrlResolver`
- file hashing
- `MediaOptions`

Provider packages register `IMediaProvider`:

- `Procyon.Media.S3` exposes `AddS3Provider(IConfiguration)`.
- `Procyon.Media.Azure` exposes `AddAzureMediaProvider(IConfiguration)`.

Important behavior:

- The core package does not depend on a specific cloud provider.
- `DefaultFolder` is prepended to generated keys when configured.
- Uploads generate GUID file names by default while preserving extensions.
- `EnableHashing` computes SHA256 and resets seekable streams before upload.
- `BaseUrl` is used by the default URL resolver.
- `IsDuplicate` is currently always false; deduplication belongs to consuming apps unless explicitly implemented.

When adding a provider, place the storage-specific implementation in its own package and register it as `IMediaProvider` after `AddProcyonMedia()`.

## Example Projects

Examples are part of the package contract. Keep them simple, runnable, and aligned with README setup snippets.

Logging example:

- Registers `AddProcyonLogging(builder.Configuration)`.
- Uses `app.UseStaticFiles()` before `app.UseProcyonLogging()`.
- Redirects `/` to `/procyon/logs`.
- Demonstrates automatic request logging, custom logs, exceptions, levels, and `[NoLog]`.

Media example:

- Calls `DotNetEnv.Env.Load()` before building configuration.
- Registers SQLite, `AddProcyonMedia(builder.Configuration)`, and `AddS3Provider(builder.Configuration)`.
- Uses controllers and Swagger.
- Applies EF Core migrations at startup for the example database.
- Demonstrates upload, list, signed URL, and delete endpoints.

Do not make examples production-heavy. Their purpose is to demonstrate package usage clearly.

## Tests

Tests use xUnit. `Procyon.Media.Tests` also references Moq.

Add or update focused tests when changing behavior, especially for:

- options binding
- file naming
- retention behavior
- queue/writer behavior
- middleware skip/logging behavior
- media path generation
- hashing behavior
- URL resolution
- provider registration

Prefer deterministic tests using temp directories and in-memory configuration. Do not require real AWS, Azure, network, or production credentials for normal test runs.

## Documentation Rules

The README files are user-facing package docs. Keep them accurate whenever public APIs, setup steps, defaults, config keys, example endpoints, or behavior changes.

Use the existing documentation style:

- concise package overview
- package table
- setup snippets
- config JSON examples
- environment variable equivalents when helpful
- runnable example commands
- endpoint lists for examples

Mention current limitations plainly, such as media deduplication not being implemented when relevant.

## Web UI Design and Layout

The current web UI is the embedded Procyon logs page in `Procyon.Logging`.

Design direction:

- utilitarian developer tool, not a marketing page
- compact, scan-friendly layout
- dark background with restrained contrast
- top header with product title and live/polling status
- table-first layout for logs
- level-specific color accents
- monospace/pre-wrapped detail cells
- minimal dependencies; SignalR client is loaded only for live updates
- no cards inside cards, no decorative gradients, no large hero sections

If expanding the UI:

- Keep the first screen focused on log inspection.
- Prioritize filters, search, level toggles, refresh/live status, clear grouping, and readable details.
- Preserve responsive behavior for narrow screens.
- Ensure text does not overlap or overflow controls.
- Keep controls dense and predictable, using buttons, inputs, selects, checkboxes/toggles, and tabs where they fit the tool.
- Do not add decorative illustrations or branding-heavy layouts.

## Package Metadata and Releases

Package projects define NuGet metadata in their `.csproj` files, including `PackageId`, `Version`, `Description`, `PackageTags`, `RepositoryUrl`, symbols, and `snupkg` format.

When changing versions:

- Update all package projects that belong to the release scope.
- Keep README claims about current versions aligned.
- Do not create a Git tag unless the user explicitly asks for it.
- If asked to tag, use a clear version tag name matching the requested release convention.

## Agent Checklist

Before changing code:

1. Read the relevant README, project file, example `Program.cs`, and nearby tests.
2. Check `git status --short`.
3. Identify whether the change affects Logging, Media, providers, examples, docs, or tests.

After changing code:

1. Run the narrowest useful build/test command.
2. Update README/config examples when public behavior or setup changes.
3. Check `git status --short`.
4. If committing, stage only task-related files and use a clear commit message.
5. Tag only when explicitly instructed by the user.
