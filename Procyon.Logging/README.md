# Procyon.Logging

Lightweight, config-driven JSON file logging for ASP.NET Core applications.

## Packages

| Package | Description |
| --- | --- |
| `Procyon.Logging.Abstractions` | Public logging contracts, log entry model, log levels, and `[NoLog]` |
| `Procyon.Logging` | File writer, queue, middleware, retention, and optional browser log page |

Current package versions are `0.1.0` while the API is still stabilizing.

## Setup

```csharp
builder.Services.AddProcyonLogging(builder.Configuration);

var app = builder.Build();

app.UseProcyonLogging();
```

`UseProcyonLogging()` adds automatic API request logging. When the app supports endpoint routing, it also maps the optional browser page and SignalR hub.

## Configuration

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
        "Path": "/procyon/logs",
        "UseSignalR": true,
        "FallbackPollingSeconds": 3
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

## File Logging

Logs are written as newline-delimited JSON. Each line is one serialized `ProcyonLogEntry`.

Default behavior:

- Folder: `logs`
- Mode: daily files
- Daily file format: `procyon-log-dd-MM-yy.json`
- Retention: enabled
- Retention window: 5 days

Set `File:Mode` to `Single` to write to one file using `File:SingleFileName`.

## Browser Log Page

The browser page is available at `/procyon/logs` by default. It is enabled by config and development-only by default, similar to a Swagger UI setup.

When `Web:UseSignalR` is true, live updates are pushed through SignalR. The page also polls `/procyon/logs/entries` as a fallback.

## Example Project

See `examples/Procyon.Logging.Example` for a runnable ASP.NET Core app that showcases:

- automatic API request logging
- JSON log files under `examples/Procyon.Logging.Example/logs`
- the browser UI at `/procyon/logs`
- SignalR live updates with polling fallback
- request body, response body, header, and query string capture
- custom `IProcyonLogger` calls
- `[NoLog]` skipping automatic request logging
- structured exception logging

Run it with:

```bash
dotnet run --project Procyon.Logging/examples/Procyon.Logging.Example/Procyon.Logging.Example.csproj
```

Then open:

```text
http://localhost:5287/procyon/logs
```

Useful sample endpoints:

```text
GET  /api/logging-demo/ping?source=browser
POST /api/logging-demo/orders
POST /api/logging-demo/levels
POST /api/logging-demo/exception
GET  /api/logging-demo/quiet
```

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

Use `[NoLog]` on a controller or action to skip automatic API logging.

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

## Notes

- This package does not depend on Serilog, NLog, or a database provider.
- Database logging is intentionally out of scope for the initial file-only package.
- Supported levels are `Trace`, `Debug`, `Information`, `Warning`, `Error`, and `Critical`.
