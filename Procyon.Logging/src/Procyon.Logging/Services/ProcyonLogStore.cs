using System.Collections.Concurrent;
using Procyon.Logging.Abstractions;

namespace Procyon.Logging.Services;

public sealed class ProcyonLogStore
{
    private const int MaxEntries = 500;
    private readonly ConcurrentQueue<ProcyonLogEntry> _entries = new();

    public void Add(ProcyonLogEntry entry)
    {
        _entries.Enqueue(entry);

        while (_entries.Count > MaxEntries && _entries.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyCollection<ProcyonLogEntry> GetRecent()
        => _entries.ToArray();
}
