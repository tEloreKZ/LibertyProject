using ProjectLiberty.Abstractions.Logging;
using System.Collections.Concurrent;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Pool for native call arguments to reduce GC pressure.
/// </summary>
public class NativeCallPool
{
    private readonly ConcurrentBag<object[]> _argumentPools = new ConcurrentBag<object[]>();
    private readonly ILogger _logger;
    private int _pooledCount;
    private int _borrowedCount;
    private int _returnedCount;

    public NativeCallPool(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Borrows an argument array from the pool.
    /// </summary>
    /// <param name="size">Array size.</param>
    /// <returns>Borrowed array.</returns>
    public object[] Borrow(int size)
    {
        if (_argumentPools.TryTake(out var args) && args.Length >= size)
        {
            _borrowedCount++;
            return args;
        }

        _pooledCount++;
        return new object[size];
    }

    /// <summary>
    /// Returns an argument array to the pool.
    /// </summary>
    /// <param name="args">Array to return.</param>
    public void Return(object[] args)
    {
        if (args == null || args.Length == 0)
            return;

        // Clear array to prevent memory leaks
        Array.Clear(args, 0, args.Length);

        _argumentPools.Add(args);
        _returnedCount++;
    }

    /// <summary>
    /// Gets pool statistics.
    /// </summary>
    public (int Pooled, int Borrowed, int Returned, int Available) GetStats()
    {
        return (_pooledCount, _borrowedCount, _returnedCount, _argumentPools.Count);
    }

    /// <summary>
    /// Clears the pool.
    /// </summary>
    public void Clear()
    {
        while (_argumentPools.TryTake(out _))
        {
        }
        _logger.LogInformation($"Cleared native call pool");
    }
}
