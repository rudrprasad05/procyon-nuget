using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Options;
using Procyon.Logging.Services;

namespace Procyon.Logging.Middleware;

public sealed class ProcyonLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ProcyonLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ProcyonLogQueue queue,
        IOptionsMonitor<ProcyonLoggingOptions> optionsMonitor)
    {
        var options = optionsMonitor.CurrentValue;

        if (!options.Enabled ||
            !options.ApiLogging.Enabled ||
            !ProcyonLogger.ShouldLog(options, ProcyonLogLevel.Information) ||
            ShouldSkipWebLogRequest(context, options) ||
            NoLogMetadata.HasNoLog(context.GetEndpoint()))
        {
            await _next(context);
            return;
        }

        var apiOptions = options.ApiLogging;
        var timestamp = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        string? requestBody = null;
        string? responseBody = null;
        Stream? originalResponseBody = null;
        MemoryStream? responseBuffer = null;

        if (apiOptions.LogRequestBody)
            requestBody = await ReadRequestBodyAsync(context.Request, apiOptions.MaxBodyLength, context.RequestAborted);

        if (apiOptions.LogResponseBody)
        {
            originalResponseBody = context.Response.Body;
            responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (responseBuffer is not null && originalResponseBody is not null)
            {
                responseBuffer.Position = 0;
                responseBody = await ReadStreamAsync(responseBuffer, apiOptions.MaxBodyLength, context.RequestAborted);
                responseBuffer.Position = 0;
                await responseBuffer.CopyToAsync(originalResponseBody, context.RequestAborted);
                context.Response.Body = originalResponseBody;
                await responseBuffer.DisposeAsync();
            }

            queue.TryEnqueue(new ProcyonLogEntry
            {
                Timestamp = timestamp,
                Level = ProcyonLogLevel.Information,
                Message = "HTTP request completed",
                Source = "api",
                TraceId = context.TraceIdentifier,
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = apiOptions.LogQueryString ? context.Request.QueryString.Value : null,
                StatusCode = context.Response.StatusCode,
                DurationMs = stopwatch.Elapsed.TotalMilliseconds,
                Headers = apiOptions.LogHeaders ? ReadHeaders(context.Request.Headers) : null,
                RequestBody = requestBody,
                ResponseBody = responseBody
            });
        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpRequest request, int maxLength, CancellationToken ct)
    {
        if (request.Body is null || !request.Body.CanRead)
            return null;

        request.EnableBuffering();
        request.Body.Position = 0;

        var body = await ReadStreamAsync(request.Body, maxLength, ct);
        request.Body.Position = 0;

        return body;
    }

    private static async Task<string> ReadStreamAsync(Stream stream, int maxLength, CancellationToken ct)
    {
        var buffer = new char[Math.Max(1, maxLength)];

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var read = await reader.ReadBlockAsync(buffer.AsMemory(0, buffer.Length), ct);
        return new string(buffer, 0, read);
    }

    private static IReadOnlyDictionary<string, string[]> ReadHeaders(IHeaderDictionary headers)
        => headers.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.Where(value => value is not null).Select(value => value!).ToArray(),
            StringComparer.OrdinalIgnoreCase);

    private static bool ShouldSkipWebLogRequest(HttpContext context, ProcyonLoggingOptions options)
    {
        if (options.Web.LogRequests)
            return false;

        var webPath = NormalizePath(options.Web.Path);
        var faviconPath = NormalizePath(options.Web.FaviconPath);

        return context.Request.Path.StartsWithSegments(webPath, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.Request.Path.Value, faviconPath.Value, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(context.Request.Path.Value, "/favicon.ico", StringComparison.OrdinalIgnoreCase);
    }

    private static PathString NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new PathString("/procyon/logs");

        return new PathString(path.StartsWith('/') ? path : "/" + path);
    }
}
