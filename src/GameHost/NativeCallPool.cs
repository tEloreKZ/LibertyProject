using ProjectLiberty.Abstractions.Logging;
using System.Collections.Concurrent;

namespace ProjectLiberty.GameHost;

/// <summary>Size-bucketed pool for temporary native argument arrays.</summary>
public sealed class NativeCallPool
{
    private readonly ConcurrentDictionary<int, ConcurrentBag<object[]>> _pools = new ConcurrentDictionary<int, ConcurrentBag<object[]>>();
    private readonly ILogger _logger;
    private int _createdCount;
    private int _borrowedCount;
    private int _returnedCount;

    public NativeCallPool(ILogger logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public object[] Borrow(int size)
    {
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
        Interlocked.Increment(ref _borrowedCount);
        var pool = _pools.GetOrAdd(size, _ => new ConcurrentBag<object[]>());
        if (pool.TryTake(out var arguments)) return arguments;
        Interlocked.Increment(ref _createdCount);
        return new object[size];
    }

    public void Return(object[] args)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        Array.Clear(args, 0, args.Length);
        _pools.GetOrAdd(args.Length, _ => new ConcurrentBag<object[]>()).Add(args);
        Interlocked.Increment(ref _returnedCount);
    }

    public (int Pooled, int Borrowed, int Returned, int Available) GetStats()
    {
        return (Volatile.Read(ref _createdCount), Volatile.Read(ref _borrowedCount),
            Volatile.Read(ref _returnedCount), _pools.Values.Sum(pool => pool.Count));
    }

    public void Clear()
    {
        var count = _pools.Values.Sum(pool => pool.Count);
        _pools.Clear();
        _logger.LogInformation($"Cleared {count} pooled native argument arrays");
    }
}
