# Performance

**Performance Guidelines and Optimization Strategies**

---

## Table of Contents

1. [Overview](#overview)
2. [Performance Budgets](#performance-budgets)
3. [Native Call Optimization](#native-call-optimization)
4. [Memory Management](#memory-management)
5. [Event Bus Optimization](#event-bus-optimization)
6. [AI Performance](#ai-performance)
7. [Rendering Performance](#rendering-performance)
8. [Profiling Tools](#profiling-tools)
9. [Best Practices](#best-practices)

---

## Overview

Project Liberty is designed for high performance in the game environment. This document outlines performance budgets, optimization strategies, and best practices.

### Performance Goals

- **Tick Time**: < 1ms average per tick
- **Memory**: < 500MB baseline
- **GC Pauses**: < 10ms
- **Native Calls**: < 1000/second average
- **Frame Rate**: Maintain 60 FPS

---

## Performance Budgets

### Tick Time Budget

```
Total Tick Budget: 1ms
├── Event Processing: 0.2ms
├── AI Updates: 0.3ms
├── Entity Management: 0.2ms
├── UI Rendering: 0.2ms
└── Other Systems: 0.1ms
```

### Memory Budget

```
Total Memory Budget: 500MB
├── Core Systems: 100MB
├── Entity Pool: 150MB
├── AI Memory: 100MB
├── UI Assets: 50MB
├── Localization Cache: 50MB
└── Other: 50MB
```

### Native Call Budget

```
Total Native Calls: < 1000/second
├── Player Operations: 200/second
├── Vehicle Operations: 300/second
├── Ped Operations: 300/second
├── World Operations: 100/second
└── Other: 100/second
```

---

## Native Call Optimization

### 1. Cache Native Hashes

**BAD**: Calculate hash every call

```csharp
// BAD - Hash calculated every time
public void TeleportPlayer(Vector3 position)
{
    _nativeInvoker.Call("SET_ENTITY_COORDS", handle, position.X, position.Y, position.Z);
}
```

**GOOD**: Cache hash

```csharp
// GOOD - Hash cached
private static readonly ulong SET_ENTITY_COORDS_HASH = Game.GenerateHash("SET_ENTITY_COORDS");

public void TeleportPlayer(Vector3 position)
{
    _nativeInvoker.CallHash(SET_ENTITY_COORDS_HASH, handle, position.X, position.Y, position.Z);
}
```

### 2. Batch Native Calls

**BAD**: Multiple individual calls

```csharp
// BAD - Individual calls
public void UpdatePlayer(IPlayer player)
{
    var health = _nativeInvoker.Call<int>("GET_ENTITY_HEALTH", player.Handle);
    var armor = _nativeInvoker.Call<int>("GET_PED_ARMOUR", player.Handle);
    var position = _nativeInvoker.Call<Vector3>("GET_ENTITY_COORDS", player.Handle);
}
```

**GOOD**: Batch related calls

```csharp
// GOOD - Batched calls
public void UpdatePlayer(IPlayer player)
{
    var data = _nativeInvoker.CallBatch(new[]
    {
        new NativeCall("GET_ENTITY_HEALTH", player.Handle),
        new NativeCall("GET_PED_ARMOUR", player.Handle),
        new NativeCall("GET_ENTITY_COORDS", player.Handle)
    });
}
```

### 3. Avoid Redundant Calls

**BAD**: Call same native multiple times

```csharp
// BAD - Redundant calls
public void Process()
{
    var pos1 = _nativeInvoker.Call<Vector3>("GET_ENTITY_COORDS", handle);
    var pos2 = _nativeInvoker.Call<Vector3>("GET_ENTITY_COORDS", handle);
    var pos3 = _nativeInvoker.Call<Vector3>("GET_ENTITY_COORDS", handle);
}
```

**GOOD**: Cache result

```csharp
// GOOD - Cached result
public void Process()
{
    var position = _nativeInvoker.Call<Vector3>("GET_ENTITY_COORDS", handle);
    // Use position multiple times
}
```

### 4. Use Native Call Pooling

```csharp
public class NativeCallPool
{
    private readonly ConcurrentBag<NativeCall> _pool = new ConcurrentBag<NativeCall>();
    
    public NativeCall Rent()
    {
        if (_pool.TryTake(out var call))
            return call;
        return new NativeCall();
    }
    
    public void Return(NativeCall call)
    {
        call.Reset();
        _pool.Add(call);
    }
}
```

---

## Memory Management

### 1. Object Pooling

**BAD**: Create new objects frequently

```csharp
// BAD - Creates new object every tick
public void OnTick()
{
    var data = new PlayerData();
    // Use data
}
```

**GOOD**: Use object pool

```csharp
// GOOD - Object pool
private readonly ObjectPool<PlayerData> _dataPool = new ObjectPool<PlayerData>();

public void OnTick()
{
    var data = _dataPool.Get();
    try
    {
        // Use data
    }
    finally
    {
        _dataPool.Return(data);
    }
}
```

### 2. Use Structs for Small Objects

**BAD**: Class for small data

```csharp
// BAD - Class for small data
public class Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
```

**GOOD**: Struct for small data

```csharp
// GOOD - Struct for small data
public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}
```

### 3. Avoid Boxing

**BAD**: Boxing value types

```csharp
// BAD - Boxing
public void Process(object value)
{
    if (value is int)
    {
        var num = (int)value; // Unboxing
    }
}
```

**GOOD**: Use generics

```csharp
// GOOD - Generics
public void Process<T>(T value)
{
    if (value is int)
    {
        var num = (int)(object)value; // No boxing if T is int
    }
}
```

### 4. Dispose Resources Properly

```csharp
public class Service : IDisposable
{
    private readonly IDisposable _resource;
    
    public Service()
    {
        _resource = new Resource();
    }
    
    public void Dispose()
    {
        _resource.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

### 5. Use Arrays Instead of Lists for Fixed Size

**BAD**: List for fixed size

```csharp
// BAD - List for fixed size
public void Process()
{
    var items = new List<int>(100);
    for (int i = 0; i < 100; i++)
    {
        items.Add(i);
    }
}
```

**GOOD**: Array for fixed size

```csharp
// GOOD - Array for fixed size
public void Process()
{
    var items = new int[100];
    for (int i = 0; i < 100; i++)
    {
        items[i] = i;
    }
}
```

---

## Event Bus Optimization

### 1. Filter Events

**BAD**: Process all events

```csharp
// BAD - Process all events
public void HandleEvent(IEvent @event)
{
    if (@event is PlayerSpawnedEvent)
    {
        // Handle
    }
    else if (@event is VehicleCreatedEvent)
    {
        // Handle
    }
    // ... many more
}
```

**GOOD**: Subscribe to specific events

```csharp
// GOOD - Specific subscription
public void Initialize()
{
    EventBus.Subscribe<PlayerSpawnedEvent>(OnPlayerSpawned);
    EventBus.Subscribe<VehicleCreatedEvent>(OnVehicleCreated);
}

private void OnPlayerSpawned(PlayerSpawnedEvent @event)
{
    // Handle
}
```

### 2. Use Event Filters

```csharp
public class HighPriorityFilter : IEventFilter<BackupRequestedEvent>
{
    public bool ShouldHandle(BackupRequestedEvent @event)
    {
        return @event.Priority == BackupPriority.High;
    }
}

EventBus.Subscribe<BackupRequestedEvent>(handler, new HighPriorityFilter());
```

### 3. Unsubscribe When Done

```csharp
public class Service : IDisposable
{
    private readonly IEventHandler<PlayerDiedEvent> _handler;
    
    public Service(IEventBus eventBus)
    {
        _handler = new PlayerDiedHandler(this);
        eventBus.Subscribe(_handler);
    }
    
    public void Dispose()
    {
        EventBus.Unsubscribe(_handler);
    }
}
```

### 4. Use Struct Events Where Possible

```csharp
// GOOD - Struct event
public struct PlayerHealthChangedEvent : IEvent
{
    public int PlayerHandle { get; }
    public int OldHealth { get; }
    public int NewHealth { get; }
}
```

---

## AI Performance

### 1. Limit AI Update Frequency

**BAD**: Update AI every tick

```csharp
// BAD - Update every tick
public void OnTick()
{
    foreach (var ai in _ais)
    {
        ai.Update();
    }
}
```

**GOOD**: Update at reduced frequency

```csharp
// GOOD - Update at reduced frequency
private float _aiUpdateTimer;

public void OnTick()
{
    _aiUpdateTimer += DeltaTime;
    if (_aiUpdateTimer >= 0.1f) // 10 times per second
    {
        _aiUpdateTimer = 0;
        foreach (var ai in _ais)
        {
            ai.Update();
        }
    }
}
```

### 2. Use Spatial Partitioning

**BAD**: Check all entities

```csharp
// BAD - Check all entities
public IEnumerable<IEntity> GetNearbyEntities(Vector3 position, float radius)
{
    return _entities.Where(e => Vector3.Distance(e.Position, position) < radius);
}
```

**GOOD**: Use spatial partitioning

```csharp
// GOOD - Spatial partitioning
public IEnumerable<IEntity> GetNearbyEntities(Vector3 position, float radius)
{
    var cell = _spatialGrid.GetCell(position);
    return cell.GetEntitiesInRadius(position, radius);
}
```

### 3. Optimize Behavior Trees

**BAD**: Rebuild tree every update

```csharp
// BAD - Rebuild every update
public void Update()
{
    var tree = BuildBehaviorTree();
    tree.Execute();
}
```

**GOOD**: Reuse tree

```csharp
// GOOD - Reuse tree
private readonly BehaviorTree _tree;

public AIController()
{
    _tree = BuildBehaviorTree();
}

public void Update()
{
    _tree.Execute();
}
```

### 4. Use Blackboard Efficiently

```csharp
// GOOD - Use struct keys
public struct BlackboardKey<T>
{
    public int Id { get; }
}

public class Blackboard
{
    private readonly Dictionary<int, object> _values = new Dictionary<int, object>();
    
    public void Set<T>(BlackboardKey<T> key, T value)
    {
        _values[key.Id] = value;
    }
    
    public T Get<T>(BlackboardKey<T> key)
    {
        return (T)_values[key.Id];
    }
}
```

---

## Rendering Performance

### 1. Batch UI Rendering

**BAD**: Render each element separately

```csharp
// BAD - Separate rendering
public void Render()
{
    foreach (var element in _elements)
    {
        element.Render();
    }
}
```

**GOOD**: Batch rendering

```csharp
// GOOD - Batched rendering
public void Render()
{
    var spriteBatch = _spriteBatch.Begin();
    foreach (var element in _elements)
    {
        element.Render(spriteBatch);
    }
    spriteBatch.End();
}
```

### 2. Use Sprite Batching

```csharp
public class SpriteBatch
{
    private readonly List<SpriteDrawCommand> _commands = new List<SpriteDrawCommand>();
    
    public void Add(SpriteDrawCommand command)
    {
        _commands.Add(command);
    }
    
    public void Flush()
    {
        // Sort by texture
        _commands.Sort((a, b) => a.Texture.CompareTo(b.Texture));
        
        // Batch draw
        foreach (var batch in _commands.GroupBy(c => c.Texture))
        {
            DrawBatch(batch);
        }
        
        _commands.Clear();
    }
}
```

### 3. Limit Draw Calls

```csharp
// GOOD - Limit draw calls
public void Render()
{
    var visibleElements = _elements.Where(e => e.IsVisible);
    var sortedElements = visibleElements.OrderBy(e => e.ZIndex);
    
    foreach (var element in sortedElements)
    {
        element.Render();
    }
}
```

---

## Profiling Tools

### 1. Performance Profiler

```csharp
public class PerformanceProfiler
{
    private readonly Dictionary<string, ProfilerEntry> _entries = new Dictionary<string, ProfilerEntry>();
    
    public IDisposable Profile(string name)
    {
        return new ProfilerScope(this, name);
    }
    
    private class ProfilerScope : IDisposable
    {
        private readonly PerformanceProfiler _profiler;
        private readonly string _name;
        private readonly Stopwatch _stopwatch;
        
        public ProfilerScope(PerformanceProfiler profiler, string name)
        {
            _profiler = profiler;
            _name = name;
            _stopwatch = Stopwatch.StartNew();
        }
        
        public void Dispose()
        {
            _stopwatch.Stop();
            _profiler.Record(_name, _stopwatch.Elapsed);
        }
    }
}
```

### 2. Memory Profiler

```csharp
public class MemoryProfiler
{
    private long _previousMemory;
    
    public void CheckMemory()
    {
        var currentMemory = GC.GetTotalMemory(false);
        var delta = currentMemory - _previousMemory;
        
        if (delta > 1024 * 1024) // More than 1MB increase
        {
            Logger.LogWarning($"Memory increased by {delta / 1024 / 1024}MB");
        }
        
        _previousMemory = currentMemory;
    }
}
```

### 3. Native Call Profiler

```csharp
public class NativeCallProfiler
{
    private readonly Dictionary<ulong, CallStats> _stats = new Dictionary<ulong, CallStats>();
    
    public void RecordCall(ulong hash, TimeSpan duration)
    {
        if (!_stats.TryGetValue(hash, out var stats))
        {
            stats = new CallStats();
            _stats[hash] = stats;
        }
        
        stats.CallCount++;
        stats.TotalTime += duration;
    }
}
```

---

## Best Practices

### 1. Profile Before Optimizing

Always profile before optimizing:

```csharp
// Profile first
using (Profiler.Profile("UpdatePlayers"))
{
    UpdatePlayers();
}

// Then optimize if needed
```

### 2. Optimize Hot Paths First

Focus on code that runs frequently:

```csharp
// HOT PATH - Optimize this
public void OnTick()
{
    // Runs 60 times per second
}

// COLD PATH - Less critical
public void OnLoad()
{
    // Runs once
}
```

### 3. Use Appropriate Data Structures

Choose the right data structure:

```csharp
// GOOD - Dictionary for lookups
private readonly Dictionary<int, IEntity> _entities = new Dictionary<int, IEntity>();

// GOOD - List for iteration
private readonly List<IEntity> _visibleEntities = new List<IEntity>();

// GOOD - HashSet for uniqueness
private readonly HashSet<int> _processedEntities = new HashSet<int>();
```

### 4. Avoid Premature Optimization

Don't optimize without measuring:

```csharp
// BAD - Premature optimization
public void Process()
{
    // Complex optimization without profiling
    var optimized = ComplexOptimization(data);
}

// GOOD - Simple first, optimize if needed
public void Process()
{
    var result = SimpleProcess(data);
    // Profile and optimize if slow
}
```

### 5. Use Async for Long Operations

```csharp
// GOOD - Async for long operations
public async Task LoadDataAsync()
{
    var data = await _fileService.ReadFileAsync("data.json");
    ProcessData(data);
}
```

---

## Summary

Performance optimization requires:

- **Profiling**: Measure before optimizing
- **Native call optimization**: Cache hashes, batch calls
- **Memory management**: Object pooling, avoid allocations
- **Event optimization**: Filter events, unsubscribe properly
- **AI optimization**: Limit update frequency, spatial partitioning
- **Rendering optimization**: Batch rendering, limit draw calls

All performance optimizations must be measured and validated. Never optimize without profiling data.
