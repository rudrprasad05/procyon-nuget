using Microsoft.Extensions.Hosting;

namespace Procyon.Logging.Services;

public sealed class ProcyonLogRetentionHostedService : BackgroundService
{
    private readonly ProcyonLogRetentionService _retentionService;

    public ProcyonLogRetentionHostedService(ProcyonLogRetentionService retentionService)
    {
        _retentionService = retentionService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _retentionService.ApplyRetentionAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
