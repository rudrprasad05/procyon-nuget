using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Procyon.Logging.Options;

namespace Procyon.Logging.Web;

public sealed class ProcyonLogHub : Hub
{
    private readonly IOptionsMonitor<ProcyonLoggingOptions> _options;
    private readonly IHostEnvironment _environment;

    public ProcyonLogHub(
        IOptionsMonitor<ProcyonLoggingOptions> options,
        IHostEnvironment environment)
    {
        _options = options;
        _environment = environment;
    }

    public override Task OnConnectedAsync()
    {
        var options = _options.CurrentValue;

        if (!options.Enabled ||
            !options.Web.Enabled ||
            options.Web.DevOnly && !_environment.IsDevelopment())
        {
            Context.Abort();
            return Task.CompletedTask;
        }

        return base.OnConnectedAsync();
    }
}
