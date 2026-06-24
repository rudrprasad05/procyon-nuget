using System.Threading.Channels;
using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Services;

public sealed class ProcyonLogQueue
{
    private readonly Channel<ProcyonLogEntry> _channel;

    public ProcyonLogQueue()
    {
        _channel = Channel.CreateBounded<ProcyonLogEntry>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool TryEnqueue(ProcyonLogEntry entry)
        => _channel.Writer.TryWrite(entry);

    public IAsyncEnumerable<ProcyonLogEntry> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}
