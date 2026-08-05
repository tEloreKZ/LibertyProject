# Project Liberty

**Modern Open Platform for GTA V Police Roleplay Modifications**

---

## Overview

Project Liberty is a next-generation, modular platform for creating police roleplay modifications for GTA V Story Mode. It is designed to be the new standard for GTA V modding, providing a clean, extensible architecture that allows developers to create plugins, callouts, and scenarios without modifying the core system.

### Key Features

- **Modular Architecture**: Every system can be disabled, replaced, or extended
- **Clean Abstraction**: Complete separation between game core and GTA V interaction
- **Plugin SDK**: Full-featured SDK for developing third-party modifications
- **Scripting API**: Write custom scenarios without touching the core
- **Event-Driven**: All systems communicate through events, no direct dependencies
- **Multi-Language Support**: Full localization support for 11+ languages
- **Hot Reload**: Development-friendly hot reload support
- **Performance Optimized**: Minimal native calls, GC, CPU, and RAM usage

### What Project Liberty Is NOT

- **NOT** an LSPDFR clone or fork
- **NOT** a quick MVP
- **NOT** dependent on a specific GTA V loader
- **NOT** limited to a single use case

---

## Architecture

Project Liberty is built on industry-standard architectural patterns:

- **Clean Architecture**: Clear separation of concerns
- **SOLID Principles**: Maintainable and extensible code
- **Dependency Injection**: Loose coupling and testability
- **Event-Driven**: Decoupled system communication
- **Domain-Driven Design**: Where appropriate

### Core Abstractions

All game systems interact through abstraction layers:

- `IGameHost` - Main game host interface
- `INativeBridge` - Native function bridge
- `IGameLoop` - Game loop management
- `IEntityManager` - Entity management
- `IPlayerManager` - Player management
- `IVehicleManager` - Vehicle management
- `IPedManager` - Ped management
- `IWorldManager` - World management
- `INativeInvoker` - Native function invocation

**Direct ScriptHookVDotNet API calls are forbidden in game systems.**

---

## Quick Start

### Prerequisites

- GTA V (Steam/Epic Games version)
- ScriptHookVDotNet 3.x
- .NET 8.0 SDK
- Visual Studio 2022 or JetBrains Rider

### Installation

1. Clone the repository:
```bash
git clone https://github.com/your-org/ProjectLiberty.git
cd ProjectLiberty
```

2. Build the solution:
```bash
dotnet build
```

3. Copy build output to your GTA V `scripts` folder

4. Launch GTA V

### Creating Your First Plugin

```csharp
using ProjectLiberty.PluginSystem;

public class MyFirstPlugin : Plugin
{
    public override string Name => "My First Plugin";
    public override string Version => "1.0.0";
    public override string Author => "Your Name";

    public override void Initialize()
    {
        // Plugin initialization logic
    }

    public override void Load()
    {
        // Plugin load logic
    }

    public override void Enable()
    {
        // Plugin enable logic
    }
}
```

See [PLUGIN_SDK.md](Documentation/PLUGIN_SDK.md) for detailed guide.

---

## Documentation

### Core Documentation

- [ARCHITECTURE.md](Documentation/ARCHITECTURE.md) - System architecture and design principles
- [PROJECT_RULES.md](Documentation/PROJECT_RULES.md) - Development rules and guidelines
- [CODING_STANDARDS.md](Documentation/CODING_STANDARDS.md) - C# coding standards
- [DIRECTORY_STRUCTURE.md](Documentation/DIRECTORY_STRUCTURE.md) - Project structure reference

### System Documentation

- [EVENT_BUS.md](Documentation/EVENT_BUS.md) - Event system architecture
- [PLUGIN_SDK.md](Documentation/PLUGIN_SDK.md) - Plugin development guide
- [SCRIPTING_API.md](Documentation/SCRIPTING_API.md) - Scripting API reference
- [LOCALIZATION.md](Documentation/LOCALIZATION.md) - Localization system
- [CONFIGURATION.md](Documentation/CONFIGURATION.md) - Configuration system
- [SAVE_SYSTEM.md](Documentation/SAVE_SYSTEM.md) - Save system architecture

### Feature Documentation

- [DISPATCH.md](Documentation/DISPATCH.md) - Dispatch system
- [TRAFFIC_STOPS.md](Documentation/TRAFFIC_STOPS.md) - Traffic stop system
- [CALL_OUTS.md](Documentation/CALL_OUTS.md) - Callout system
- [ARREST_SYSTEM.md](Documentation/ARREST_SYSTEM.md) - Arrest system
- [AI_SYSTEM.md](Documentation/AI_SYSTEM.md) - AI architecture

### Additional Documentation

- [PERFORMANCE.md](Documentation/PERFORMANCE.md) - Performance guidelines
- [ROADMAP.md](Documentation/ROADMAP.md) - Development roadmap
- [CONTRIBUTING.md](Documentation/CONTRIBUTING.md) - Contribution guidelines
- [LICENSE.md](Documentation/LICENSE.md) - Project license

---

## Project Structure

```
ProjectLiberty/
├── src/                    # Source code
│   ├── Core/              # Core systems
│   ├── Abstractions/      # Interface definitions
│   ├── GameHost/          # Game host implementation
│   ├── PluginSystem/      # Plugin system
│   ├── EventBus/          # Event bus
│   ├── Localization/      # Localization
│   ├── Configuration/     # Configuration
│   ├── SaveSystem/        # Save system
│   ├── Logging/           # Logging
│   ├── Dispatch/          # Dispatch system
│   ├── TrafficStops/      # Traffic stops
│   ├── Callouts/          # Callout system
│   ├── ArrestSystem/      # Arrest system
│   ├── AI/                # AI system
│   └── ...                # Other systems
├── SDK/                   # SDK for developers
│   ├── Plugins/           # Plugin SDK
│   ├── Callouts/          # Callout SDK
│   ├── Scripts/           # Scripting SDK
│   ├── Examples/          # Example code
│   └── Templates/         # Project templates
├── Documentation/         # Documentation
├── Tests/                 # Tests
├── Languages/             # Localization files
├── Config/                # Configuration files
├── Plugins/               # Installed plugins
└── Scripts/               # Installed scripts
```

See [DIRECTORY_STRUCTURE.md](Documentation/DIRECTORY_STRUCTURE.md) for complete structure.

---

## Supported Languages

- English (en)
- Russian (ru)
- French (fr)
- German (de)
- Spanish (es)
- Portuguese (pt)
- Italian (it)
- Polish (pl)
- Ukrainian (uk)
- Chinese (zh)
- Japanese (ja)
- Korean (ko)

Additional languages can be added without code changes.

---

## Development Status

Project Liberty is currently in **Architecture Design Phase**.

See [ROADMAP.md](Documentation/ROADMAP.md) for detailed development plan.

---

## Contributing

We welcome contributions! Please read [CONTRIBUTING.md](Documentation/CONTRIBUTING.md) for guidelines.

---

## License

This project is licensed under the MIT License - see [LICENSE.md](Documentation/LICENSE.md) for details.

---

## Contact

- **Project Lead**: [Your Name]
- **Discord**: [Server Link]
- **GitHub**: [Repository Link]

---

## Acknowledgments

- Rockstar Games for GTA V
- ScriptHookVDotNet team for the scripting framework
- The GTA V modding community

---

**Project Liberty - Setting the New Standard for GTA V Police Modding**
