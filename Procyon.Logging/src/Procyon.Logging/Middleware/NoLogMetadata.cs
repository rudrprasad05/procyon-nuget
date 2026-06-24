using Microsoft.AspNetCore.Http;
using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Middleware;

internal static class NoLogMetadata
{
    public static bool HasNoLog(Endpoint? endpoint)
        => endpoint?.Metadata.GetMetadata<NoLogAttribute>() is not null;
}
