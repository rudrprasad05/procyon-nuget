using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Services;

internal sealed class NullProcyonLogBroadcaster : IProcyonLogBroadcaster
{
    public Task BroadcastAsync(ProcyonLogEntry entry, CancellationToken ct)
        => Task.CompletedTask;
}
