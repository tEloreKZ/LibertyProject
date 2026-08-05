@echo off
echo Building Project Liberty...

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed.
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

echo Restoring packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

echo Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Failed to build solution
    pause
    exit /b 1
)

echo Copying DLLs to output folder...
if exist "output" rmdir /s /q "output"
mkdir "output"

copy "src\Abstractions\bin\Release\net8.0\ProjectLiberty.Abstractions.dll" "output\" >nul
copy "src\Core\bin\Release\net8.0\ProjectLiberty.Core.dll" "output\" >nul
copy "src\GameHost\bin\Release\net8.0\ProjectLiberty.GameHost.dll" "output\" >nul
copy "src\EventBus\bin\Release\net8.0\ProjectLiberty.EventBus.dll" "output\" >nul
copy "src\PluginSystem\bin\Release\net8.0\ProjectLiberty.PluginSystem.dll" "output\" >nul
copy "src\Localization\bin\Release\net8.0\ProjectLiberty.Localization.dll" "output\" >nul
copy "src\Configuration\bin\Release\net8.0\ProjectLiberty.Configuration.dll" "output\" >nul
copy "src\Logging\bin\Release\net8.0\ProjectLiberty.Logging.dll" "output\" >nul
copy "src\EntryPoint\bin\Release\net8.0\ProjectLiberty.EntryPoint.dll" "output\" >nul
copy "src\EntryPoint\bin\Release\net8.0\ProjectLiberty.EntryPoint.exe" "output\" >nul
copy "src\EntryPoint\bin\Release\net8.0\ProjectLiberty.EntryPoint.runtimeconfig.json" "output\" >nul
copy "src\EntryPoint\bin\Release\net8.0\ProjectLiberty.EntryPoint.deps.json" "output\" >nul

if exist "src\LibertyScript\bin\Release\net48\LibertyScript.dll" (
    copy "src\LibertyScript\bin\Release\net48\LibertyScript.dll" "output\" >nul
)

if exist "Plugins\ExamplePlugin\bin\Release\net8.0\ExamplePlugin.dll" (
    copy "Plugins\ExamplePlugin\bin\Release\net8.0\ExamplePlugin.dll" "output\" >nul
)

echo Copying configuration files...
mkdir "output\Config"
copy "Config\core.json" "output\Config\" >nul

mkdir "output\Languages"
mkdir "output\Languages\en"
mkdir "output\Languages\ru"
copy "Languages\en\core.json" "output\Languages\en\" >nul
copy "Languages\ru\core.json" "output\Languages\ru\" >nul

echo Creating scripts deployment folder...
mkdir "output\scripts"
copy "src\Abstractions\bin\Release\net8.0\ProjectLiberty.Abstractions.dll" "output\scripts\" >nul
copy "src\Core\bin\Release\net8.0\ProjectLiberty.Core.dll" "output\scripts\" >nul
copy "src\GameHost\bin\Release\net8.0\ProjectLiberty.GameHost.dll" "output\scripts\" >nul
copy "src\EventBus\bin\Release\net8.0\ProjectLiberty.EventBus.dll" "output\scripts\" >nul
copy "src\PluginSystem\bin\Release\net8.0\ProjectLiberty.PluginSystem.dll" "output\scripts\" >nul
copy "src\Localization\bin\Release\net8.0\ProjectLiberty.Localization.dll" "output\scripts\" >nul
copy "src\Configuration\bin\Release\net8.0\ProjectLiberty.Configuration.dll" "output\scripts\" >nul
copy "src\Logging\bin\Release\net8.0\ProjectLiberty.Logging.dll" "output\scripts\" >nul

if exist "src\LibertyScript\bin\Release\net48\LibertyScript.dll" (
    copy "src\LibertyScript\bin\Release\net48\LibertyScript.dll" "output\scripts\" >nul
)

if exist "Plugins\ExamplePlugin\bin\Release\net8.0\ExamplePlugin.dll" (
    copy "Plugins\ExamplePlugin\bin\Release\net8.0\ExamplePlugin.dll" "output\scripts\" >nul
)

mkdir "output\scripts\Config"
copy "Config\core.json" "output\scripts\Config\" >nul

mkdir "output\scripts\Languages"
mkdir "output\scripts\Languages\en"
mkdir "output\scripts\Languages\ru"
copy "Languages\en\core.json" "output\scripts\Languages\en\" >nul
copy "Languages\ru\core.json" "output\scripts\Languages\ru\" >nul

echo Build completed successfully!
echo.
echo All files copied to: output\
echo.
pause
