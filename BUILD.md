# Build Instructions

## Prerequisites

1. **Install .NET Framework 4.8**
   - Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48
   - Usually pre-installed on Windows 10/11

2. **Install .NET 8.0 SDK** (for build tools)
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

3. **Install Visual Studio 2022** (optional but recommended)
   - Download from: https://visualstudio.microsoft.com/downloads/
   - Required workloads: .NET desktop development

## Building the Solution

### Using build.bat (Recommended)

```bash
cd G:\ProjectLiberty\LibertyProject
build.bat
```

This will:
- Restore NuGet packages
- Build all projects in Release configuration
- Copy all DLLs and configuration files to `output\` folder
- Copy deployment files to `output\scripts\` folder for GTA V

### Using .NET CLI

```bash
cd G:\ProjectLiberty\LibertyProject
dotnet restore
dotnet build --configuration Release
dotnet test Tests\ProjectLiberty.Tests.csproj --configuration Release --no-build --no-restore
```

### Using Visual Studio

1. Open `ProjectLiberty.sln` in Visual Studio 2022
2. Select `Release` configuration
3. Build → Build Solution (Ctrl+Shift+B)

## Target Framework

All projects target **.NET Framework 4.8** for ScriptHookVDotNet compatibility:
- Abstractions, Core, GameHost, EventBus, PluginSystem, Localization, Configuration, Logging, EntryPoint, LibertyScript

## Output

Built assemblies will be in:
- `src\*\bin\Release\net48\`

Deployment files will be in:
- `output\` - All DLLs, EXE, config files for standalone testing
- `output\scripts\` - All DLLs, config files for GTA V deployment

## Deployment

### For GTA V (ScriptHookVDotNet)

1. Install ScriptHookVDotNet3 for GTA V
2. Copy all files from `output\scripts\` to `GTA V\scripts\`
3. Launch GTA V
4. LibertyScript will automatically initialize Project Liberty

### For Standalone Testing

1. Run `output\ProjectLiberty.EntryPoint.exe`
2. This will initialize Project Liberty outside of GTA V

## Configuration Files

Ensure these files exist in the application directory:
- `Config/core.json` - Core configuration
- `Languages/en/core.json` - English translations
- `Languages/ru/core.json` - Russian translations

## Dependencies

- **ScriptHookVDotNet3** (v3.6.0) - GTA V scripting framework
- **Newtonsoft.Json** (v13.0.3) - JSON serialization

## Plugin Development

To build the test plugin:

```bash
cd Plugins\TestPlugin
dotnet build --configuration Release
```

Copy the built DLL to the `Plugins` folder for automatic loading.

## Architecture

Project Liberty uses a modular architecture with the following components:
- **Abstractions** - Core interfaces and contracts
- **Core** - Main application logic and dependency injection
- **GameHost** - Game loop and entity management
- **EventBus** - Event-driven communication
- **PluginSystem** - Plugin loading and management
- **Localization** - Multi-language support
- **Configuration** - Configuration management with hot reload
- **Logging** - File and console logging
- **EntryPoint** - Standalone entry point for testing
- **LibertyScript** - ScriptHookVDotNet integration script
