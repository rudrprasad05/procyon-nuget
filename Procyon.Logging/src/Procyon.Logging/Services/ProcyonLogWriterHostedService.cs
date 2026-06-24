using Microsoft.Extensions.Hosting;

namespace Procyon.Logging.Services;

internal sealed class ProcyonLogWriterHostedService : BackgroundService
{
    private readonly ProcyonLogQueue _queue;
    private readonly ProcyonFileLogWriter _fileLogWriter;
    private readonly ProcyonLogStore _store;
    private readonly IProcyonLogBroadcaster _broadcaster;

    public ProcyonLogWriterHostedService(
        ProcyonLogQueue queue,
        ProcyonFileLogWriter fileLogWriter,
        ProcyonLogStore store,
        IProcyonLogBroadcaster broadcaster)
    {
        _queue = queue;
        _fileLogWriter = fileLogWriter;
        _store = store;
        _broadcaster = broadcaster;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var entry in _queue.ReadAllAsync(stoppingToken))
            {
                await _fileLogWriter.WriteAsync(entry, stoppingToken);
                _store.Add(entry);
                await _broadcaster.BroadcastAsync(entry, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}
