using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Options;

namespace Procyon.Logging.Services;

public sealed class ProcyonFileLogWriter
{
    private readonly IOptionsMonitor<ProcyonLoggingOptions> _options;
    private readonly IHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public ProcyonFileLogWriter(
        IOptionsMonitor<ProcyonLoggingOptions> options,
        IHostEnvironment environment)
    {
        _options = options;
        _environment = environment;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task WriteAsync(ProcyonLogEntry entry, CancellationToken ct = default)
    {
        var options = _options.CurrentValue;

        if (!options.Enabled || !options.File.Enabled)
            return;

        var filePath = GetLogFilePath(entry.Timestamp);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)!);

        var json = JsonSerializer.Serialize(entry, _jsonOptions);

        await _writeLock.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(filePath, json + Environment.NewLine, ct);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public string GetLogFilePath(DateTimeOffset timestamp)
    {
        var fileOptions = _options.CurrentValue.File;
        var directory = ResolveLogDirectory(fileOptions.Path);
        var fileName = fileOptions.Mode == ProcyonLogFileMode.Single
            ? fileOptions.SingleFileName
            : $"procyon-log-{timestamp.ToString(fileOptions.DateFileFormat, CultureInfo.InvariantCulture)}.json";

        return System.IO.Path.Combine(directory, fileName);
    }

    public string ResolveLogDirectory(string configuredPath)
    {
        if (System.IO.Path.IsPathRooted(configuredPath))
            return configuredPath;

        return System.IO.Path.Combine(_environment.ContentRootPath, configuredPath);
    }
}
