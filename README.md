# Project Liberty

Project Liberty is an early-stage modular framework for police-roleplay modifications in GTA V Story Mode. The repository currently contains a working core foundation, ScriptHookVDotNet integration, configuration and localization services, an event bus, an in-process plugin system, basic UI prototypes, and automated infrastructure tests.

> Status: core stabilization. Dispatch, traffic stops, callouts, arrests, AI, saves, Lua scripting, and production UI are planned but not implemented.

## Current capabilities

- .NET Framework 4.8 runtime assemblies built with the .NET 8 SDK
- abstraction boundary around the game host and native access
- game loop explicitly advanced on the GTA main thread
- synchronous and asynchronous event delivery with filters and priorities
- trusted in-process plugin discovery, dependency ordering, and lifecycle management
- dotted-path JSON configuration with atomic save and debounced hot reload
- JSON localization with English and Russian resources
- file and console logging
- basic ScriptHookVDotNet menu prototype
- standalone mock host and xUnit tests

## Important limitations

- Plugins are trusted .NET assemblies loaded into the main `AppDomain`. They are not sandboxed and cannot be truly unloaded without restarting the process.
- Only ScriptHookVDotNet 3.x is implemented as a real game adapter today.
- Hot reload applies to configuration files, not plugin assemblies or scripts.
- The UI is a prototype and the planned gameplay systems are not present yet.
- In-game behavior still requires manual validation in GTA V.

## Architecture

```text
LibertyScript (ScriptHookVDotNet entry point)
    -> Core / Application lifecycle
        -> Abstractions
        -> GameHost
        -> EventBus
        -> PluginSystem
        -> Configuration / Localization / Logging
        -> UI
```

Game-facing code advances `IGameLoop` from the ScriptHook tick. Core services and plugins should use abstractions instead of directly referencing ScriptHookVDotNet.

## Prerequisites

- Windows and .NET Framework 4.8
- .NET 8 SDK
- GTA V and ScriptHookVDotNet 3.x for in-game use
- Visual Studio 2022 or Rider is optional

## Build and test

```powershell
dotnet restore ProjectLiberty.sln
dotnet build ProjectLiberty.sln --configuration Release --no-restore
dotnet test Tests/ProjectLiberty.Tests.csproj --configuration Release --no-build --no-restore
```

On Windows, `build.bat` performs restore, Release build, tests, and creates the deployment layout under `output\scripts`.

## GTA V deployment

1. Run `build.bat`.
2. Copy the contents of `output\scripts` into the GTA V `scripts` directory.
3. Ensure ScriptHookVDotNet 3.x is installed.
4. Start GTA V. Project Liberty initializes through `LibertyScript.dll`.

F2 toggles the current prototype menu.

## Plugin layout

Plugins use one directory per plugin:

```text
ProjectLiberty/Plugins/MyPlugin/MyPlugin.dll
```

The assembly may contain one or more concrete `IPlugin` implementations with parameterless constructors. Treat plugins as fully trusted code. See [Plugin SDK](Documentation/PLUGIN_SDK.md) for the current contract.

## Configuration and localization

Core configuration lives in `Config/core.json` and is addressed with dotted keys such as `game.tickRate`. English and Russian translations live under `Languages/en` and `Languages/ru`.

## Repository map

- `src/Abstractions` — public contracts
- `src/Core` — application lifecycle and composition
- `src/GameHost` — game-host adapters and loop
- `src/EventBus` — event delivery
- `src/PluginSystem` — trusted plugin loading and lifecycle
- `src/Configuration`, `Localization`, `Logging` — infrastructure services
- `src/UI`, `LibertyScript` — ScriptHook-facing UI and entry point
- `Tests` — automated core tests
- `Documentation` — design documents; some describe planned architecture

## Roadmap

The current roadmap and the distinction between implemented and planned features are maintained in [ROADMAP.md](Documentation/ROADMAP.md).

## License

Licensed under the [MIT License](LICENSE.md).
