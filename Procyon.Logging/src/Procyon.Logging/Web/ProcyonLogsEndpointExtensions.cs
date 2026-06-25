using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Procyon.Logging.Options;
using Procyon.Logging.Services;

namespace Procyon.Logging.Web;

public static class ProcyonLogsEndpointExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    static ProcyonLogsEndpointExtensions()
    {
        JsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static IEndpointRouteBuilder MapProcyonLogs(this IEndpointRouteBuilder endpoints)
    {
        var configuredOptions = endpoints.ServiceProvider
            .GetRequiredService<IOptions<ProcyonLoggingOptions>>()
            .Value;

        var basePath = NormalizePath(configuredOptions.Web.Path);
        var entriesPath = $"{basePath}/entries";
        var hubPath = $"{basePath}/hub";
        var faviconPath = NormalizePath(configuredOptions.Web.FaviconPath);

        endpoints.MapGet(basePath, async context =>
        {
            if (!IsAllowed(context))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var options = context.RequestServices.GetRequiredService<IOptionsMonitor<ProcyonLoggingOptions>>().CurrentValue;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(RenderPage(options.Web, entriesPath, hubPath, faviconPath), context.RequestAborted);
        });

        endpoints.MapGet(entriesPath, async context =>
        {
            if (!IsAllowed(context))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var store = context.RequestServices.GetRequiredService<ProcyonLogStore>();
            context.Response.ContentType = "application/json; charset=utf-8";
            await JsonSerializer.SerializeAsync(context.Response.Body, store.GetRecent(), JsonOptions, context.RequestAborted);
        });

        endpoints.MapGet(faviconPath, async context =>
        {
            if (!IsAllowed(context))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "image/svg+xml; charset=utf-8";
            await context.Response.WriteAsync(DefaultFaviconSvg, context.RequestAborted);
        });

        if (configuredOptions.Web.UseSignalR)
            endpoints.MapHub<ProcyonLogHub>(hubPath);

        return endpoints;
    }

    private static bool IsAllowed(HttpContext context)
    {
        var options = context.RequestServices.GetRequiredService<IOptionsMonitor<ProcyonLoggingOptions>>().CurrentValue;
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

        if (!options.Enabled || !options.Web.Enabled)
            return false;

        return !options.Web.DevOnly || environment.IsDevelopment();
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/procyon/logs";

        return path.StartsWith('/') ? path : "/" + path;
    }

    private static string RenderPage(ProcyonLoggingWebOptions options, string entriesPath, string hubPath, string faviconPath)
        => $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Procyon Logs</title>
  <link rel="icon" type="image/svg+xml" href="{{faviconPath}}">
  <style>
    :root { color-scheme: light dark; font-family: ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
    body { margin: 0; background: #111827; color: #f9fafb; }
    header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; border-bottom: 1px solid #374151; }
    h1 { margin: 0; font-size: 18px; font-weight: 650; }
    main { padding: 16px 20px; }
    table { width: 100%; border-collapse: collapse; font-size: 13px; }
    th, td { padding: 10px; border-bottom: 1px solid #263244; text-align: left; vertical-align: top; }
    th { color: #bfdbfe; font-weight: 650; }
    code { white-space: pre-wrap; word-break: break-word; }
    .status { color: #93c5fd; font-size: 13px; }
    .level-Error, .level-Critical { color: #fca5a5; }
    .level-Warning { color: #fcd34d; }
    .level-Information { color: #a7f3d0; }
    .level-Debug, .level-Trace { color: #c4b5fd; }
  </style>
</head>
<body>
  <header>
    <h1>Procyon Logs</h1>
    <span id="status" class="status">connecting</span>
  </header>
  <main>
    <table>
      <thead>
        <tr>
          <th>Time</th>
          <th>Level</th>
          <th>Message</th>
          <th>Request</th>
          <th>Details</th>
        </tr>
      </thead>
      <tbody id="logs"></tbody>
    </table>
  </main>
  <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.7/signalr.min.js"></script>
  <script>
    const entriesUrl = "{{entriesPath}}";
    const hubUrl = "{{hubPath}}";
    const useSignalR = {{options.UseSignalR.ToString().ToLowerInvariant()}};
    const pollingMs = {{Math.Max(1, options.FallbackPollingSeconds)}} * 1000;
    const rows = document.getElementById("logs");
    const status = document.getElementById("status");
    const seen = new Set();
    const levelNames = {
      0: "Trace",
      1: "Debug",
      2: "Information",
      3: "Warning",
      4: "Error",
      5: "Critical"
    };

    function key(entry) {
      return [entry.timestamp, entry.traceId, entry.message, entry.durationMs].join("|");
    }

    function add(entry) {
      const id = key(entry);
      if (seen.has(id)) return;
      seen.add(id);

      const level = formatLevel(entry.level);
      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${escapeHtml(entry.timestamp ?? "")}</td>
        <td class="level-${escapeHtml(level)}">${escapeHtml(level)}</td>
        <td>${escapeHtml(entry.message ?? "")}</td>
        <td><code>${escapeHtml([entry.method, entry.path, entry.statusCode, entry.durationMs ? entry.durationMs.toFixed(2) + "ms" : ""].filter(Boolean).join(" "))}</code></td>
        <td><code>${escapeHtml(JSON.stringify(entry.data ?? entry.exception ?? {}, null, 2))}</code></td>`;
      rows.prepend(tr);
    }

    function formatLevel(value) {
      if (value === null || value === undefined) return "";
      if (typeof value === "number") return levelNames[value] ?? String(value);
      if (/^\d+$/.test(value)) return levelNames[Number(value)] ?? value;
      return value;
    }

    function escapeHtml(value) {
      return String(value).replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
    }

    async function poll() {
      const response = await fetch(entriesUrl, { cache: "no-store" });
      if (!response.ok) return;
      const entries = await response.json();
      entries.forEach(add);
    }

    async function start() {
      await poll();
      if (useSignalR && window.signalR) {
        const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).withAutomaticReconnect().build();
        connection.on("procyonLog", add);
        try {
          await connection.start();
          status.textContent = "live";
          setInterval(poll, pollingMs);
          return;
        } catch {
        }
      }
      status.textContent = "polling";
      setInterval(poll, pollingMs);
    }

    start();
  </script>
</body>
</html>
""";

    private const string DefaultFaviconSvg = """
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 64 64">
  <rect width="64" height="64" rx="14" fill="#111827"/>
  <path d="M17 18h30v28H17z" fill="#f9fafb"/>
  <path d="M22 24h20M22 31h16M22 38h20" stroke="#111827" stroke-width="3" stroke-linecap="round"/>
  <circle cx="48" cy="18" r="7" fill="#38bdf8"/>
</svg>
""";
}
