using Microsoft.AspNetCore.SignalR;
using Procyon.Logging.Abstractions;
using Procyon.Logging.Web;

namespace Procyon.Logging.Services;

internal sealed class SignalRProcyonLogBroadcaster : IProcyonLogBroadcaster
{
    private readonly IHubContext<ProcyonLogHub> _hubContext;

    public SignalRProcyonLogBroadcaster(IHubContext<ProcyonLogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastAsync(ProcyonLogEntry entry, CancellationToken ct)
        => _hubContext.Clients.All.SendAsync("procyonLog", entry, ct);
}
