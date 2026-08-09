# Procyon.Logging

Lightweight, config-driven JSON file logging for ASP.NET Core applications.

## Overview

`Procyon.Logging` adds automatic API request logging, structured custom logs, newline-delimited JSON file output, retention cleanup, and an optional browser log page with SignalR live updates.

It does not require Serilog, NLog, a database provider, or an external log service.

## Packages

| Package | Description |
| --- | --- |
| `Procyon.Logging.Abstractions` | Public contracts, `ProcyonLogEntry`, log levels, exception model, and `[NoLog]`. |
| `Procyon.Logging` | Middleware, queue, file writer, retention services, web log page, and SignalR broadcaster. |

Current package versions are `0.1.0` while the API is still stabilizing.

## Setup

```csharp
builder.Services.AddProcyonLogging(builder.Configuration);

var app = builder.Build();

app.UseStaticFiles();
app.UseProcyonLogging();
app.UseProcyonLoggingUi();
```

`UseProcyonLogging()` adds automatic API request logging. `UseProcyonLoggingUi()` maps the optional browser page, log entries endpoint, favicon endpoint, and SignalR hub using the configured `Web:Path`. Calling both is safe; UI endpoints are mapped once.

## Configuration

Configuration is read from `Procyon:Logging`.

```json
{
  "Procyon": {
    "Logging": {
      "Enabled": true,
      "MinimumLevel": "Information",
      "File": {
        "Enabled": true,
        "Path": "logs",
        "Mode": "Daily",
        "SingleFileName": "procyon-log.json",
        "DateFileFormat": "dd-MM-yy",
        "RetentionEnabled": true,
        "RetainDays": 5
      },
      "Web": {
        "Enabled": true,
        "DevOnly": true,
        "Path": "/procyon/logging",
        "LogRequests": false,
        "UseSignalR": true,
        "FallbackPollingSeconds": 3,
        "FaviconPath": "/procyon/logging/favicon.svg"
      },
      "ApiLogging": {
        "Enabled": true,
        "LogRequestBody": false,
        "LogResponseBody": false,
        "LogHeaders": false,
        "LogQueryString": true,
        "MaxBodyLength": 4096
      }
    }
  }
}
```

Supported levels are `Trace`, `Debug`, `Information`, `Warning`, `Error`, and `Critical`.

## File Logging

Logs are written as newline-delimited JSON. Each line is one serialized `ProcyonLogEntry`.

Default file behavior:

- Folder: `logs`
- Mode: daily files
- Daily file format: `procyon-log-dd-MM-yy.json`
- Retention: enabled
- Retention window: 5 days

Set `File:Mode` to `Single` to write to one file using `File:SingleFileName`.

## Browser Log Page

The browser page is available at `/procyon/logging` by default. It is enabled by config and development-only by default, similar to a Swagger UI setup.

When `Web:UseSignalR` is true, live entries are pushed through SignalR. The page also polls the entries endpoint as a fallback using `Web:FallbackPollingSeconds`.

## API Auto-Logging

The middleware logs:

- HTTP method
- Path
- Status code
- Duration
- Timestamp
- Trace id
- Query string by default

Request bodies, response bodies, and headers are off by default and can be enabled in config.

Use `[NoLog]` on a controller or action to skip automatic API logging for that endpoint. Custom `IProcyonLogger` calls still work inside `[NoLog]` endpoints.

## Custom Logger

Inject `IProcyonLogger` from `Procyon.Logging.Abstractions`.

```csharp
public class UploadService
{
    private readonly IProcyonLogger _logger;

    public UploadService(IProcyonLogger logger)
    {
        _logger = logger;
    }

    public void Complete(Guid userId)
    {
        _logger.Info("Upload completed", new { userId });
    }

    public void Fail(Exception exception, string fileName)
    {
        _logger.Error(exception, "Upload failed", new { fileName });
    }
}
```

Custom logger calls enqueue entries through a background channel and return without waiting for file I/O.

## Example Project

The runnable example is in `examples/Procyon.Logging.Example`.

```bash
dotnet run --project Procyon.Logging/examples/Procyon.Logging.Example/Procyon.Logging.Example.csproj
```

Then open:

```text
http://localhost:5287/procyon/logging
```

Useful sample endpoints:

```text
GET  /api/logging-demo/ping?source=browser
POST /api/logging-demo/orders
POST /api/logging-demo/levels
POST /api/logging-demo/exception
GET  /api/logging-demo/quiet
```

The example demonstrates automatic request logging, custom logs, structured exception logging, body/header/query capture, `[NoLog]`, file output, SignalR live updates, and polling fallback.
