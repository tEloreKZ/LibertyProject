using ProjectLiberty.Abstractions.Logging;
using System.Collections.Concurrent;
using GTA.Native;

namespace ProjectLiberty.GameHost;

/// <summary>
/// Cache for native function hashes to avoid repeated string-to-hash conversions.
/// </summary>
public class NativeHashCache
{
    private readonly ConcurrentDictionary<string, ulong> _cache = new ConcurrentDictionary<string, ulong>();
    private readonly ILogger _logger;
    private readonly bool _useScriptHook;
    private int _hits;
    private int _misses;

    public NativeHashCache(ILogger logger, bool useScriptHook = true)
    {
        _logger = logger;
        _useScriptHook = useScriptHook;
    }

    /// <summary>
    /// Gets the hash for a native function name.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <returns>Native hash.</returns>
    public ulong GetHash(string nativeName)
    {
        if (_cache.TryGetValue(nativeName, out var hash))
        {
            Interlocked.Increment(ref _hits);
            return hash;
        }

        Interlocked.Increment(ref _misses);

        if (_useScriptHook)
        {
            try
            {
                hash = GetScriptHookHash(nativeName);
            }
            catch (Exception ex)
            {
                throw new KeyNotFoundException($"Unknown GTA native name: {nativeName}", ex);
            }
        }
        else
        {
            hash = ComputeSimpleHash(nativeName);
        }

        _cache[nativeName] = hash;
        _logger.LogDebug($"Cached native hash: {nativeName} -> 0x{hash:X}");
        return hash;
    }

    /// <summary>
    /// Gets hash from ScriptHookVDotNet using reflection.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <returns>Native hash.</returns>
    private static ulong GetScriptHookHash(string nativeName)
    {
        if (string.IsNullOrWhiteSpace(nativeName)) throw new ArgumentException("Native name cannot be empty.", nameof(nativeName));
        if (!Enum.TryParse(nativeName, true, out Hash nativeHash))
            throw new KeyNotFoundException($"Unknown GTA native name: {nativeName}");
        return (ulong)nativeHash;
    }

    /// <summary>
    /// Computes a simple hash for standalone mode.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <returns>Simple hash.</returns>
    private ulong ComputeSimpleHash(string nativeName)
    {
        // Jenkins one-at-a-time hash algorithm
        uint hash = 0;
        foreach (var c in nativeName)
        {
            hash += (uint)c;
            hash += (hash << 10);
            hash ^= (hash >> 6);
        }
        hash += (hash << 3);
        hash ^= (hash >> 11);
        hash += (hash << 15);
        return hash;
    }

    /// <summary>
    /// Checks if a hash is cached.
    /// </summary>
    /// <param name="nativeName">Native function name.</param>
    /// <returns>True if cached, false otherwise.</returns>
    public bool IsCached(string nativeName)
    {
        return _cache.ContainsKey(nativeName);
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation($"Cleared {count} native hashes from cache");
    }

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    public (int Cached, int Hits, int Misses, double HitRate) GetStats()
    {
        var hits = Volatile.Read(ref _hits);
        var misses = Volatile.Read(ref _misses);
        var total = hits + misses;
        var hitRate = total > 0 ? (double)hits / total : 0.0;
        return (_cache.Count, hits, misses, hitRate);
    }
}
