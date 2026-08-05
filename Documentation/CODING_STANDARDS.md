# Coding Standards

**C# Coding Standards for Project Liberty**

---

## Table of Contents

1. [General Rules](#general-rules)
2. [Naming Conventions](#naming-conventions)
3. [File Organization](#file-organization)
4. [Code Style](#code-style)
5. [C# Specific Guidelines](#c-specific-guidelines)
6. [Documentation Standards](#documentation-standards)
7. [Best Practices](#best-practices)
8. [Anti-Patterns](#anti-patterns)

---

## General Rules

### 1. Code Formatting

- Use 4 spaces for indentation (no tabs)
- Maximum line length: 120 characters
- One statement per line
- One declaration per line
- Use blank lines between logical sections

### 2. File Encoding

- Use UTF-8 encoding
- Use CRLF line endings (Windows)
- Include BOM for UTF-8

### 3. EditorConfig

Project includes `.editorconfig` for consistent formatting:

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

[*.{csproj,sln}]
indent_style = space
indent_size = 2
```

---

## Naming Conventions

### 1. Classes and Structs

**PascalCase** - Noun or noun phrase

```csharp
public class PlayerManager
{
}

public struct Vector3
{
}
```

### 2. Interfaces

**PascalCase** with `I` prefix

```csharp
public interface IPlayerManager
{
}

public interface IVehicle
{
}
```

### 3. Methods

**PascalCase** - Verb or verb phrase

```csharp
public void CreateVehicle()
{
}

public bool IsValid()
{
}
```

### 4. Properties

**PascalCase** - Noun or noun phrase

```csharp
public int Health { get; set; }
public string Name { get; }
public bool IsEnabled { get; private set; }
```

### 5. Fields

**camelCase** with underscore prefix for private fields

```csharp
private int _health;
private readonly string _name;
private static readonly int _maxHealth = 100;
```

### 6. Constants

**PascalCase**

```csharp
public const int MaxHealth = 100;
public const string DefaultLanguage = "en";
```

### 7. Local Variables

**camelCase**

```csharp
var playerPosition = new Vector3();
int vehicleCount = 0;
```

### 8. Parameters

**camelCase**

```csharp
public void CreateVehicle(VehicleModel model, Vector3 position, float heading)
{
}
```

### 9. Events

**PascalCase** - Verb or verb phrase, past tense for events that occurred

```csharp
public event EventHandler<PlayerSpawnedEventArgs> PlayerSpawned;
public event EventHandler<PlayerDiedEventArgs> PlayerDied;
```

### 10. Delegates

**PascalCase** with `EventHandler` suffix for event handlers

```csharp
public delegate void PlayerSpawnedEventHandler(object sender, PlayerSpawnedEventArgs e);
```

### 11. Enums

**PascalCase** for enum type, **PascalCase** for values

```csharp
public enum VehicleModel
{
    Police,
    Police2,
    Sheriff,
    Sheriff2
}
```

### 12. Namespaces

**PascalCase** - Hierarchical, dot-separated

```csharp
namespace ProjectLiberty.Core
{
}

namespace ProjectLiberty.PluginSystem
{
}
```

---

## File Organization

### 1. One Class Per File

Each file should contain only one public class.

### 2. File Naming

File name should match class name:

```
PlayerManager.cs
IVehicle.cs
Vector3.cs
```

### 3. File Structure

```csharp
using System;
using ProjectLiberty.Abstractions;

namespace ProjectLiberty.Core
{
    /// <summary>
    /// Summary of class.
    /// </summary>
    public class PlayerManager : IPlayerManager
    {
        // Constants
        // Fields
        // Properties
        // Constructors
        // Public Methods
        // Private Methods
        // Events
    }
}
```

### 4. Using Directives

- Place at top of file
- Sort alphabetically
- Remove unused usings
- Use alias for conflicting names

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using ProjectLiberty.Abstractions;
using ProjectLiberty.Events;
```

### 5. Region Usage

Use regions sparingly, only for large files:

```csharp
#region Public Methods

public void Method1() { }
public void Method2() { }

#endregion
```

---

## Code Style

### 1. Braces

Allman style - opening brace on new line:

```csharp
public void Method()
{
    if (condition)
    {
        DoSomething();
    }
}
```

### 2. Spacing

- Space after comma
- Space after semicolon in for loops
- Space around operators
- No space after method name

```csharp
var result = method(arg1, arg2);
for (int i = 0; i < 10; i++)
{
    sum += i;
}
```

### 3. Conditional Statements

Always use braces, even for single statements:

```csharp
// GOOD
if (condition)
{
    DoSomething();
}

// BAD
if (condition)
    DoSomething();
```

### 4. Switch Statements

Include default case, break explicitly:

```csharp
switch (value)
{
    case 1:
        DoSomething();
        break;
    case 2:
        DoSomethingElse();
        break;
    default:
        throw new ArgumentOutOfRangeException();
}
```

### 5. LINQ Formatting

Multi-line LINQ:

```csharp
var result = items
    .Where(item => item.IsValid)
    .OrderBy(item => item.Name)
    .Select(item => item.Value)
    .ToList();
```

---

## C# Specific Guidelines

### 1. Var Usage

Use `var` when type is obvious:

```csharp
// GOOD - Type obvious
var player = new Player();
var count = players.Count;

// GOOD - Complex type
var result = SomeComplexMethod();

// BAD - Type not obvious
var data = GetData(); // What is data?
```

### 2. Null Handling

Use null-conditional operators:

```csharp
// GOOD
player?.Vehicle?.Speed;

// GOOD
if (player != null)
{
    player.DoSomething();
}

// BAD
if (player == null || player.Vehicle == null)
{
    return;
}
player.Vehicle.Speed = 100;
```

### 3. String Interpolation

Use string interpolation over concatenation:

```csharp
// GOOD
string message = $"Player {player.Name} has {player.Health} health";

// BAD
string message = "Player " + player.Name + " has " + player.Health + " health";
```

### 4. String Comparison

Use ordinal comparison for performance:

```csharp
// GOOD
if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))

// BAD
if (a.ToLower() == b.ToLower())
```

### 5. Object Initialization

Use object initializer syntax:

```csharp
// GOOD
var player = new Player
{
    Name = "John",
    Health = 100
};

// BAD
var player = new Player();
player.Name = "John";
player.Health = 100;
```

### 6. Collection Initialization

Use collection initializer:

```csharp
// GOOD
var list = new List<int> { 1, 2, 3 };

// BAD
var list = new List<int>();
list.Add(1);
list.Add(2);
list.Add(3);
```

### 7. Properties vs Fields

Always use properties, never public fields:

```csharp
// GOOD
public class Player
{
    public int Health { get; set; }
}

// BAD
public class Player
{
    public int Health;
}
```

### 8. Async/Await

Use async/await properly:

```csharp
// GOOD
public async Task<Result> GetDataAsync()
{
    var result = await _service.GetDataAsync();
    return result;
}

// BAD
public async Task<Result> GetDataAsync()
{
    return _service.GetDataAsync().Result;
}
```

### 9. Exception Handling

Catch specific exceptions:

```csharp
// GOOD
try
{
    DoSomething();
}
catch (InvalidOperationException ex)
{
    _logger.LogError(ex, "Operation failed");
}

// BAD
try
{
    DoSomething();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
}
```

### 10. Dispose Pattern

Implement IDisposable properly:

```csharp
public class ResourceManager : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }

            // Dispose unmanaged resources
            _disposed = true;
        }
    }
}
```

---

## Documentation Standards

### 1. XML Documentation

All public members must have XML documentation:

```csharp
/// <summary>
/// Manages player-related operations.
/// </summary>
public interface IPlayerManager
{
    /// <summary>
    /// Gets the local player instance.
    /// </summary>
    /// <returns>The local player, or null if not available.</returns>
    IPlayer GetLocalPlayer();

    /// <summary>
    /// Spawns a player at the specified position.
    /// </summary>
    /// <param name="position">The spawn position.</param>
    /// <param name="heading">The spawn heading in degrees.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the player cannot be spawned.
    /// </exception>
    void SpawnPlayer(Vector3 position, float heading);
}
```

### 2. Comments

Use comments to explain **why**, not **what**:

```csharp
// GOOD - Explains why
// We need to delay this because the game hasn't loaded the model yet
await Task.Delay(1000);

// BAD - Explains what (obvious from code)
// Create a new player
var player = new Player();
```

### 3. TODO Comments

Use TODO for future work:

```csharp
// TODO: Implement proper error handling
// TODO: Optimize this loop for performance
// TODO: Add unit tests for this method
```

---

## Best Practices

### 1. Method Length

Keep methods short - maximum 50 lines:

```csharp
// GOOD - Short, focused method
public void CreateVehicle(VehicleModel model, Vector3 position)
{
    ValidateModel(model);
    ValidatePosition(position);
    
    var vehicle = _vehicleFactory.Create(model, position);
    _entityManager.Add(vehicle);
    
    PublishVehicleCreatedEvent(vehicle);
}
```

### 2. Parameter Count

Maximum 4 parameters, use parameter object for more:

```csharp
// GOOD - Few parameters
public void CreateVehicle(VehicleModel model, Vector3 position, float heading)
{
}

// GOOD - Parameter object for many parameters
public void CreateVehicle(VehicleCreationOptions options)
{
}

// BAD - Too many parameters
public void CreateVehicle(VehicleModel model, Vector3 position, float heading, Color color, bool isSirenOn, int livery)
{
}
```

### 3. Class Complexity

Keep classes focused - single responsibility:

```csharp
// GOOD - Focused class
public class PlayerSpawner
{
    public IPlayer Spawn(Vector3 position, float heading)
    {
        // Spawn logic only
    }
}

// BAD - Too many responsibilities
public class PlayerManager
{
    public IPlayer Spawn() { }
    public void Save() { }
    public void Load() { }
    public void DrawUI() { }
    public void HandleInput() { }
}
```

### 4. Magic Numbers

Use named constants:

```csharp
// GOOD
private const float DEFAULT_HEADING = 0f;
private const int MAX_HEALTH = 100;

if (health > MAX_HEALTH)
{
    health = MAX_HEALTH;
}

// BAD
if (health > 100)
{
    health = 100;
}
```

### 5. Early Returns

Use early returns to reduce nesting:

```csharp
// GOOD
public void Process(Player player)
{
    if (player == null)
    {
        return;
    }

    if (!player.IsValid)
    {
        return;
    }

    // Main logic
}

// BAD
public void Process(Player player)
{
    if (player != null)
    {
        if (player.IsValid)
        {
            // Main logic
        }
    }
}
```

### 6. Guard Clauses

Use guard clauses for validation:

```csharp
public void CreateVehicle(VehicleModel model, Vector3 position)
{
    _ = model ?? throw new ArgumentNullException(nameof(model));
    _ = position ?? throw new ArgumentNullException(nameof(position));
    
    // Method logic
}
```

---

## Anti-Patterns

### 1. God Objects

**AVOID**: Classes that do too much

```csharp
// BAD - God object
public class GameSystem
{
    public void HandlePlayer() { }
    public void HandleVehicles() { }
    public void HandlePeds() { }
    public void HandleUI() { }
    public void HandleInput() { }
    public void HandleAudio() { }
}
```

### 2. Shotgun Surgery

**AVOID**: Changes requiring many files

```csharp
// BAD - Adding a new vehicle type requires changes everywhere
public class VehicleManager
{
    public void CreatePolice() { }
    public void CreateSheriff() { }
    public void CreateAmbulance() { }
}
```

### 3. Primitive Obsession

**AVOID**: Using primitives instead of domain types

```csharp
// BAD
public void SetPosition(float x, float y, float z)

// GOOD
public void SetPosition(Vector3 position)
```

### 4. Feature Envy

**AVOID**: Classes that use another class more than their own data

```csharp
// BAD
public class PlayerUI
{
    public void Draw()
    {
        var player = _game.GetPlayer();
        var vehicle = player.Vehicle;
        var speed = vehicle.Speed;
        // ... lots of vehicle operations
    }
}
```

### 5. Lazy Class

**AVOID**: Classes that do nothing

```csharp
// BAD
public class EmptyManager
{
    public void DoNothing()
    {
        // Literally nothing
    }
}
```

---

## Tooling

### 1. Static Analysis

Use Roslyn Analyzers for code quality:
- StyleCop.Analyzers
- Microsoft.CodeAnalysis.FxCopAnalyzers
- SonarAnalyzer.CSharp

### 2. Code Formatting

Use dotnet-format for consistent formatting:
```bash
dotnet format
```

### 3. Linting

Use CSharpier for code style:
```bash
dotnet tool install -g csharpier
dotnet csharpier .
```

---

## Summary

Following these coding standards ensures:

- **Consistency**: Uniform code across the project
- **Readability**: Easy to understand and maintain
- **Quality**: Fewer bugs and better performance
- **Collaboration**: Easier for team members to work together

All code must pass static analysis and formatting checks before merging.
