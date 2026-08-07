# Deployment Instructions

## Prerequisites

1. **GTA V** - Steam or Epic Games version
2. **ScriptHookVDotNet3** - Version 3.6.0 or higher
3. **Project Liberty** - Built assemblies

## Installation

### Step 1: Install ScriptHookVDotNet3

1. Download ScriptHookVDotNet3 from: https://github.com/crosire/scripthookvdotnet/releases
2. Extract the archive
3. Copy all files to your GTA V root directory (where GTA5.exe is located)
4. Launch GTA V once to ensure ScriptHookVDotNet is working

### Step 2: Build Project Liberty

```bash
cd G:\ProjectLiberty\LibertyProject
build.bat
```

Or manually:
```bash
dotnet restore
dotnet build --configuration Release
```

### Step 3: Deploy Project Liberty

1. Navigate to your GTA V scripts folder:
   - `GTA V\scripts\`

2. Create Project Liberty directory structure:
   ```
   scripts\
   ├── Config\
   ├── Languages\
   │   ├── en\
   │   └── ru\
   ├── Logs\
   ├── Plugins\
   └── Saves\
   ```

3. Copy built assemblies to `scripts\`:
   - `src\Abstraction\bin\Release\ProjectLiberty.Abstractions.dll`
   - `src\Core\bin\Release\ProjectLiberty.Core.dll`
   - `src\GameHost\bin\Release\ProjectLiberty.GameHost.dll`
   - `src\EventBus\bin\Release\ProjectLiberty.EventBus.dll`
   - `src\PluginSystem\bin\Release\ProjectLiberty.PluginSystem.dll`
   - `src\Localization\bin\Release\ProjectLiberty.Localization.dll`
   - `src\Configuration\bin\Release\ProjectLiberty.Configuration.dll`
   - `src\Logging\bin\Release\ProjectLiberty.Logging.dll`
   - `src\EntryPoint\bin\Release\ProjectLiberty.EntryPoint.dll`
   - `Plugins\ExamplePlugin\bin\Release\ExamplePlugin.dll`

4. Copy configuration files:
   - `Config\core.json` → `scripts\Config\core.json`

5. Copy localization files:
   - `Languages\en\core.json` → `scripts\Languages\en\core.json`
   - `Languages\ru\core.json` → `scripts\Languages\ru\core.json`

### Step 4: Deploy Example Plugin

1. Copy `Plugins\ExamplePlugin\bin\Release\ExamplePlugin.dll` to `scripts\Plugins\`
2. Create plugin manifest (optional, auto-generated from class attributes)

### Step 5: Launch GTA V

1. Launch GTA V
2. Project Liberty will automatically load
3. You should see a notification: "Project Liberty initialized successfully"
4. Check `scripts\Logs\` for log files

## Troubleshooting

### Project Liberty doesn't load

1. Check that ScriptHookVDotNet3 is installed correctly
2. Check that all required DLLs are in the scripts folder
3. Check log files in `scripts\Logs\`
4. Ensure .NET 8.0 Runtime is installed on your system

### Plugin doesn't load

1. Check plugin DLL is in `scripts\Plugins\`
2. Check plugin dependencies are met
3. Check log files for plugin-specific errors

### Configuration not loading

1. Verify `Config\core.json` exists and is valid JSON
2. Check log files for configuration errors

### Localization not working

1. Verify language directories exist
2. Verify translation files exist and are valid JSON
3. Check that default language is set in configuration

## File Structure After Installation

```
GTA V\
├── GTA5.exe
├── scripts\
│   ├── ScriptHookVDotNet.ini
│   ├── ProjectLiberty.Abstractions.dll
│   ├── ProjectLiberty.Core.dll
│   ├── ProjectLiberty.GameHost.dll
│   ├── ProjectLiberty.EventBus.dll
│   ├── ProjectLiberty.PluginSystem.dll
│   ├── ProjectLiberty.Localization.dll
│   ├── ProjectLiberty.Configuration.dll
│   ├── ProjectLiberty.Logging.dll
│   ├── ProjectLiberty.EntryPoint.dll
│   ├── Config\
│   │   └── core.json
│   ├── Languages\
│   │   ├── en\
│   │   │   └── core.json
│   │   └── ru\
│   │       └── core.json
│   ├── Logs\
│   │   └── log_*.log
│   ├── Plugins\
│   │   └── ExamplePlugin.dll
│   └── Saves\
```

## Uninstallation

1. Delete all Project Liberty DLLs from scripts folder
2. Delete Config, Languages, Logs, Plugins, Saves directories
3. Delete log files

## Development Mode

For development, you can use symbolic links instead of copying files:

```powershell
# Create symbolic link for Config
New-Item -ItemType SymbolicLink -Path "GTA V\scripts\Config" -Target "G:\ProjectLiberty\LibertyProject\Config"

# Create symbolic link for Languages
New-Item -ItemType SymbolicLink -Path "GTA V\scripts\Languages" -Target "G:\ProjectLiberty\LibertyProject\Languages"

# Create symbolic link for Plugins
New-Item -ItemType SymbolicLink -Path "GTA V\scripts\Plugins" -Target "G:\ProjectLiberty\LibertyProject\Plugins"
```

This allows you to modify files in the project directory and have them immediately available in-game.
# GTA V Enhanced installation

Build the complete package and install it with the repository installer:

```powershell
cmd /c build.bat
powershell -ExecutionPolicy Bypass -File Build\Install-Gta.ps1 `
  -GtaPath "D:\SteamLibrary\steamapps\common\Grand Theft Auto V Enhanced"
```

The installed layout is intentionally consolidated:

```text
Grand Theft Auto V Enhanced/
├── ProjectLiberty.Core.asi
└── scripts/
    ├── LibertyScript.dll
    └── ProjectLiberty/
        ├── Core/
        ├── Native/
        ├── UI/
        ├── Config/
        ├── Languages/
        ├── Plugins/
        ├── Saves/
        └── Logs/
```

The installer migrates `Saves`, `Logs`, and `Cache` from the obsolete root
`ProjectLiberty` directory before removing that duplicate directory.
