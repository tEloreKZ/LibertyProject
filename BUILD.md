# Build Instructions

## Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

2. **Install Visual Studio 2022** (optional but recommended)
   - Download from: https://visualstudio.microsoft.com/downloads/
   - Required workloads: .NET desktop development

## Building the Solution

### Using .NET CLI

```bash
cd G:\ProjectLiberty\LibertyProject
dotnet restore
dotnet build --configuration Release
```

### Using Visual Studio

1. Open `ProjectLiberty.sln` in Visual Studio 2022
2. Select `Release` configuration
3. Build → Build Solution (Ctrl+Shift+B)

## Output

Built assemblies will be in:
- `src\Abstraction\bin\Release\`
- `src\Core\bin\Release\`
- `src\GameHost\bin\Release\`
- `src\EventBus\bin\Release\`
- `src\PluginSystem\bin\Release\`
- `src\Localization\bin\Release\`
- `src\Configuration\bin\Release\`
- `src\Logging\bin\Release\`
- `src\EntryPoint\bin\Release\`

## Testing

### Unit Tests (not yet implemented)

```bash
dotnet test
```

### Manual Testing with ScriptHookVDotNet

1. Install ScriptHookVDotNet for GTA V
2. Copy built assemblies to GTA V scripts folder
3. Copy configuration files to GTA V folder
4. Launch GTA V

## Configuration Files

Ensure these files exist in the application directory:
- `Config/core.json` - Core configuration
- `Languages/en/core.json` - English translations
- `Languages/ru/core.json` - Russian translations

## Plugin Development

To build the example plugin:

```bash
cd Plugins\ExamplePlugin
dotnet build --configuration Release
```

Copy the built DLL to the `Plugins` folder for automatic loading.
