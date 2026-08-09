using Microsoft.AspNetCore.Http;

namespace Procyon.Logging.Options;

internal static class ProcyonLoggingDefaults
{
    public const string WebPath = "/procyon/logging";
    public const string WebFaviconPath = "/procyon/logging/favicon.svg";

    public static string NormalizePath(string? path, string fallback)
    {
        if (string.IsNullOrWhiteSpace(path))
            return fallback;

        return path.StartsWith('/') ? path : "/" + path;
    }

    public static PathString NormalizePathString(string? path, string fallback)
        => new(NormalizePath(path, fallback));
}
