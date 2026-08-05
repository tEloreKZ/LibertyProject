# Project Rules

**Development Rules and Guidelines for Project Liberty**

---

## Table of Contents

1. [Core Principles](#core-principles)
2. [Code Quality Rules](#code-quality-rules)
3. [Architecture Rules](#architecture-rules)
4. [Development Workflow](#development-workflow)
5. [Testing Rules](#testing-rules)
6. [Documentation Rules](#documentation-rules)
7. [Git Rules](#git-rules)
8. [Performance Rules](#performance-rules)
9. [Security Rules](#security-rules)
10. [Plugin Development Rules](#plugin-development-rules)

---

## Core Principles

### 1. Design First, Code Later

- **NEVER** write code without design
- Create documentation before implementation
- Design interfaces before implementations
- Review architecture before coding

### 2. No Quick Fixes

- **NEVER** write "quick and dirty" code
- **NEVER** create MVPs "just to work"
- Every feature must be properly designed
- Every feature must be properly tested

### 3. AAA Quality Standards

- Code must meet professional standards
- Documentation must be comprehensive
- Architecture must be solid
- Performance must be optimized

### 4. Independence from Game Loader

- Core must not depend on ScriptHookVDotNet
- All GTA V interaction through abstractions
- Must work with any compatible loader
- Easy to port to new loaders

---

## Code Quality Rules

### 1. No Direct Native Calls

**FORBIDDEN**:
```csharp
// BAD - Direct ScriptHookVDotNet call
Function.Call(Hash.GET_PLAYER_PED, playerId);
```

**REQUIRED**:
```csharp
// GOOD - Through abstraction
nativeInvoker.CallHash(NativeHash.GET_PLAYER_PED, playerId);
```

### 2. No Hardcoded Strings

**FORBIDDEN**:
```csharp
// BAD - Hardcoded string
string message = "Player spawned";
```

**REQUIRED**:
```csharp
// GOOD - Through localization
string message = localization.GetString("PlayerSpawned");
```

### 3. No Magic Numbers

**FORBIDDEN**:
```csharp
// BAD - Magic number
if (health > 100) { }
```

**REQUIRED**:
```csharp
// GOOD - Named constant
if (health > MAX_HEALTH) { }
```

### 4. No Singleton Abuse

**FORBIDDEN**:
```csharp
// BAD - Singleton
public static class GameInstance
{
    public static Player Player { get; set; }
}
```

**REQUIRED**:
```csharp
// GOOD - Dependency injection
public class GameService
{
    private readonly IPlayerManager playerManager;
    
    public GameService(IPlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }
}
```

### 5. No Static State

**FORBIDDEN**:
```csharp
// BAD - Static state
public static class State
{
    public static bool IsEnabled;
}
```

**REQUIRED**:
```csharp
// GOOD - Instance state
public class Service
{
    public bool IsEnabled { get; set; }
}
```

### 6. No Global Variables

**FORBIDDEN**:
```csharp
// BAD - Global variable
public static List<Vehicle> AllVehicles = new List<Vehicle>();
```

**REQUIRED**:
```csharp
// GOOD - Service with dependency
public class VehicleRepository
{
    private readonly List<Vehicle> vehicles = new List<Vehicle>();
    
    public IEnumerable<Vehicle> GetAll() => vehicles;
}
```

---

## Architecture Rules

### 1. Abstraction Layer

- All game systems must use abstractions
- No direct GTA V API calls in business logic
- Abstractions defined in `Abstractions` namespace
- Implementations in respective modules

### 2. Dependency Direction

**ALLOWED**:
```
Domain ← Application ← Infrastructure
```

**FORBIDDEN**:
```
Domain → Infrastructure
Application → Infrastructure (except interfaces)
```

### 3. Module Boundaries

- Each module is self-contained
- Modules communicate through events
- No direct module-to-module dependencies
- Clear public interfaces

### 4. Interface Segregation

- Interfaces must be focused
- No "god interfaces"
- Clients depend only on what they use
- Split large interfaces

---

## Development Workflow

### 1. Feature Development Process

1. **Design Phase**
   - Create design document
   - Define interfaces
   - Review with team
   - Get approval

2. **Documentation Phase**
   - Write technical documentation
   - Update relevant docs
   - Create examples
   - Review documentation

3. **Implementation Phase**
   - Write unit tests first
   - Implement interfaces
   - Write integration tests
   - Code review

4. **Testing Phase**
   - Run all tests
   - Performance testing
   - Manual testing
   - Bug fixes

5. **Release Phase**
   - Update changelog
   - Tag version
   - Create release notes
   - Deploy

### 2. Code Review Requirements

- All code must be reviewed
- At least one approval required
- All comments must be addressed
- Tests must pass
- Documentation must be updated

### 3. Branch Strategy

- `main` - Production code
- `develop` - Integration branch
- `feature/*` - Feature branches
- `bugfix/*` - Bug fix branches
- `hotfix/*` - Emergency fixes

---

## Testing Rules

### 1. Test Coverage

- Minimum 80% code coverage
- Critical paths 100% coverage
- All public methods tested
- All edge cases tested

### 2. Test Types

- **Unit Tests**: Test individual components
- **Integration Tests**: Test component interaction
- **System Tests**: Test entire system
- **Performance Tests**: Test performance characteristics

### 3. Test Naming

```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Test]
public void CreateVehicle_WithValidModel_ReturnsVehicle()
{
    // Arrange
    var model = VehicleModel.Police;
    var position = new Vector3(0, 0, 0);
    
    // Act
    var vehicle = vehicleManager.CreateVehicle(model, position, 0);
    
    // Assert
    Assert.IsNotNull(vehicle);
    Assert.AreEqual(model, vehicle.Model);
}
```

### 4. Test Organization

```
Tests/
├── Unit/
│   ├── Core/
│   ├── PluginSystem/
│   └── EventBus/
├── Integration/
│   ├── ManagerIntegration/
│   └── PluginIntegration/
└── System/
    ├── EndToEnd/
    └── Performance/
```

---

## Documentation Rules

### 1. Documentation Requirements

- All public APIs documented
- All complex logic documented
- All architecture decisions documented
- All breaking changes documented

### 2. XML Documentation

```csharp
/// <summary>
/// Creates a vehicle at the specified position.
/// </summary>
/// <param name="model">The vehicle model to create.</param>
/// <param name="position">The position where the vehicle will be spawned.</param>
/// <param name="heading">The heading of the vehicle.</param>
/// <returns>The created vehicle instance.</returns>
/// <exception cref="InvalidOperationException">
/// Thrown when the vehicle model is invalid.
/// </exception>
public IVehicle CreateVehicle(VehicleModel model, Vector3 position, float heading)
{
    // Implementation
}
```

### 3. Documentation Updates

- Documentation updated with code
- No undocumented features
- Examples provided for APIs
- Diagrams for complex systems

---

## Git Rules

### 1. Commit Messages

**Format**:
```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code style
- `refactor`: Refactoring
- `test`: Tests
- `chore`: Maintenance

**Example**:
```
feat(player): add player teleportation

Add ability to teleport player to specified position.
Includes validation and event publishing.

Closes #123
```

### 2. Branch Naming

```
feature/feature-name
bugfix/bug-description
hotfix/urgent-fix
release/version-number
```

### 3. Merge Rules

- No merge commits to main
- Use rebase for feature branches
- Squash commits for clean history
- Resolve conflicts locally

---

## Performance Rules

### 1. Native Call Optimization

- Cache native hashes
- Batch native calls
- Avoid redundant calls
- Use native call pooling

### 2. Memory Management

- Object pooling for frequent objects
- Avoid allocations in hot paths
- Use structs for small objects
- Dispose of resources properly

### 3. Tick Optimization

- Minimize tick time
- Use delta time
- Avoid blocking operations
- Async where appropriate

### 4. Performance Budgets

- Tick time: < 1ms average
- Memory: < 500MB baseline
- GC pauses: < 10ms
- Native calls: < 1000/second

---

## Security Rules

### 1. Plugin Security

- Validate all plugin inputs
- Sandbox plugin execution
- Limit plugin permissions
- Audit plugin access

### 2. Input Validation

- Validate all user input
- Validate configuration files
- Validate native parameters
- Sanitize file paths

### 3. Error Handling

- Never expose internal details
- Log errors securely
- Handle exceptions gracefully
- No crash on plugin errors

---

## Plugin Development Rules

### 1. Plugin Requirements

- Must implement `IPlugin`
- Must have manifest file
- Must support localization
- Must have configuration
- Must handle lifecycle properly

### 2. Plugin Dependencies

- Declare all dependencies
- Use version ranges
- Handle missing dependencies
- Provide fallback behavior

### 3. Plugin Events

- Subscribe to events in `Enable`
- Unsubscribe in `Disable`
- Handle event errors
- Don't block event handlers

### 4. Plugin Resources

- Clean up resources in `Disable`
- Dispose of objects
- Close file handles
- Release native resources

---

## Code Review Checklist

### Before Submitting

- [ ] Code follows coding standards
- [ ] All tests pass
- [ ] Documentation updated
- [ ] No hardcoded strings
- [ ] No direct native calls
- [ ] No singletons (unless justified)
- [ ] No static state
- [ ] Performance considered
- [ ] Security considered
- [ ] Error handling implemented

### During Review

- [ ] Architecture sound
- [ ] Code readable
- [ ] Tests adequate
- [ ] Documentation complete
- [ ] No obvious bugs
- [ ] Performance acceptable
- [ ] Security adequate

---

## Violation Consequences

### Minor Violations

- Warning in code review
- Request for fix
- Document in issue tracker

### Major Violations

- Reject pull request
- Required rewrite
- Team discussion
- Process improvement

### Critical Violations

- Immediate revert
- Root cause analysis
- Process changes
- Team training

---

## Continuous Improvement

### Regular Reviews

- Monthly architecture review
- Quarterly process review
- Annual technology review
- Continuous feedback

### Process Updates

- Document all changes
- Train team on changes
- Monitor effectiveness
- Adjust as needed

---

## Summary

These rules ensure Project Liberty maintains:

- **Quality**: Professional-grade code
- **Maintainability**: Easy to understand and modify
- **Performance**: Optimized for game environment
- **Security**: Safe for users and developers
- **Extensibility**: Easy to add new features

Following these rules is mandatory for all contributors.
