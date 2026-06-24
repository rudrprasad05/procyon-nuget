using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Services;

internal interface IProcyonLogBroadcaster
{
    Task BroadcastAsync(ProcyonLogEntry entry, CancellationToken ct);
}
