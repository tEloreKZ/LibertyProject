# Project Liberty

Project Liberty is an early-stage modular framework for police-roleplay modifications in GTA V Story Mode. The repository currently contains a working core foundation, ScriptHookVDotNet integration, configuration and localization services, an event bus, an in-process plugin system, a React-based in-game UI, and automated infrastructure tests.

> Status: core stabilization. Dispatch, traffic stops, callouts, arrests, AI, Lua scripting, and the remaining gameplay sections are planned but not implemented.

## Current capabilities

- .NET Framework 4.8 runtime assemblies built with the .NET 8 SDK
- abstraction boundary around the game host and native access
- game loop explicitly advanced on the GTA main thread
- synchronous and asynchronous event delivery with filters and priorities
- trusted in-process plugin discovery, dependency ordering, and lifecycle management
- dotted-path JSON configuration with atomic save and debounced hot reload
- JSON localization with English and Russian resources
- file and console logging
- full-screen React + TypeScript dashboard matching the approved visual design
- independent native CEF OSR service with a D3D11 renderer, native input and managed Browser API
- three persistent GTA Online freemode character slots and a React-based mouse-driven character editor
- standalone mock host and xUnit tests

## Important limitations

- Plugins are trusted .NET assemblies loaded into the main `AppDomain`. They are not sandboxed and cannot be truly unloaded without restarting the process.
- Only ScriptHookVDotNet 3.x is implemented as a real game adapter today.
- Hot reload applies to configuration files, not plugin assemblies or scripts.
- Dashboard sections beyond the main screen are visual placeholders until their gameplay systems are implemented.
- The production F2 dashboard requires the independent native CEF service.
- In-game behavior still requires manual validation in GTA V.

## Architecture

```text
ProjectLiberty.Core.asi (independent native lifecycle)
    -> CEF runtime / subprocess / BrowserManager
    -> D3D11 OSR composition + native input
    <-> ProjectLiberty.Managed BrowserId API
        <-> LibertyScript / managed plugins

LibertyScript (ScriptHookVDotNet gameplay entry point)
    -> Core / Application lifecycle
        -> Abstractions
        -> GameHost
        -> EventBus
        -> PluginSystem
        -> Configuration / Localization / Logging
        -> UI
```

Game-facing code advances `IGameLoop` from the ScriptHook tick. CEF events are queued natively and dispatched by managed clients on that thread. Core services and plugins should use abstractions instead of directly referencing ScriptHookVDotNet. See [native CEF](Documentation/CEF.md) and [UI architecture](Documentation/UI_ARCHITECTURE.md).

## Prerequisites

- Windows and .NET Framework 4.8
- .NET 8 SDK
- Node.js 20 or newer for building the React UI
- GTA V and ScriptHookVDotNet 3.x for in-game use
- Visual Studio C++ build tools with CMake

## Build and test

```powershell
dotnet restore ProjectLiberty.sln
cd src/WebUI
npm ci
npm run build
cd ../..
dotnet build ProjectLiberty.sln --configuration Release --no-restore
dotnet test Tests/ProjectLiberty.Tests.csproj --configuration Release --no-build --no-restore
```

On Windows, `build.bat` performs the React, managed and native Release builds, runs tests, and creates the complete deployment layout under `output\gta-root`.

## GTA V deployment

1. Run `build.bat`.
2. Copy the contents of `output\gta-root` into the GTA V root directory.
3. Ensure an ASI loader, ScriptHookV and ScriptHookVDotNet 3.x are installed.
4. Select Windowed or Borderless Windowed display mode in GTA V.
5. Start GTA V. Project Liberty initializes through `LibertyScript.dll`.

Press F10 to open the native CEF round-trip test. See [CEF documentation](Documentation/CEF.md) for its expected behavior and diagnostics.

F2 toggles the full-screen Project Liberty dashboard. If there are no saved characters, the dashboard opens directly into the create-character state. Up to three GTA Online freemode characters can be created, saved, and selected from the character cards. Character data is stored in `scripts/ProjectLiberty/Saves/characters.json`.

The editor uses the detailed Rockford Hills mansion interior, mouse clicks for parameters, mouse-wheel scrolling in long option lists, drag-to-rotate, and wheel zoom over the model. Only clothing slots changed by the player are forced onto the GTA freemode model, preventing invalid default masks or props. Escape returns to the dashboard without saving. Unimplemented dashboard sections display a future-phase notice.

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
- `src/LibertyScript` — ScriptHook entry point, character editor world/camera logic, and managed CEF client
- `src/WebUI` — React + TypeScript application served by native CEF
- `Tests` — automated core tests
- `Documentation` — design documents; some describe planned architecture

## Roadmap

The current roadmap and the distinction between implemented and planned features are maintained in [ROADMAP.md](Documentation/ROADMAP.md).

## License

Licensed under the [MIT License](LICENSE.md).
