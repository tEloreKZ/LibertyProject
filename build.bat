@echo off
echo Building Project Liberty...

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed.
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    if not defined CI pause
    exit /b 1
)

echo Restoring packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    if not defined CI pause
    exit /b 1
)

echo Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Failed to build solution
    if not defined CI pause
    exit /b 1
)

echo Running tests...
dotnet test "Tests\ProjectLiberty.Tests.csproj" --configuration Release --no-build --no-restore
if %errorlevel% neq 0 (
    echo ERROR: Tests failed
    if not defined CI pause
    exit /b 1
)

echo Copying DLLs to output folder...
if exist "output" rmdir /s /q "output"
mkdir "output"

copy "src\Abstractions\bin\Release\net48\ProjectLiberty.Abstractions.dll" "output\" >nul
copy "src\Core\bin\Release\net48\ProjectLiberty.Core.dll" "output\" >nul
copy "src\GameHost\bin\Release\net48\ProjectLiberty.GameHost.dll" "output\" >nul
copy "src\EventBus\bin\Release\net48\ProjectLiberty.EventBus.dll" "output\" >nul
copy "src\PluginSystem\bin\Release\net48\ProjectLiberty.PluginSystem.dll" "output\" >nul
copy "src\Localization\bin\Release\net48\ProjectLiberty.Localization.dll" "output\" >nul
copy "src\Configuration\bin\Release\net48\ProjectLiberty.Configuration.dll" "output\" >nul
copy "src\Logging\bin\Release\net48\ProjectLiberty.Logging.dll" "output\" >nul
copy "src\EntryPoint\bin\Release\net48\ProjectLiberty.EntryPoint.exe" "output\" >nul
copy "src\EntryPoint\bin\Release\net48\ProjectLiberty.EntryPoint.exe.config" "output\" >nul

if exist "src\LibertyScript\bin\Release\net48\LibertyScript.dll" (
    copy "src\LibertyScript\bin\Release\net48\LibertyScript.dll" "output\" >nul
)

echo Copying Newtonsoft.Json.dll...
copy "src\LibertyScript\bin\Release\net48\Newtonsoft.Json.dll" "output\" >nul
copy "src\LibertyScript\bin\Release\net48\ScriptHookVDotNet3.dll" "output\" >nul

if exist "Plugins\TestPlugin\bin\Release\net48\TestPlugin.dll" (
    mkdir "output\Plugins"
    mkdir "output\Plugins\TestPlugin"
    copy "Plugins\TestPlugin\bin\Release\net48\TestPlugin.dll" "output\Plugins\TestPlugin\" >nul
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
mkdir "output\scripts\ProjectLiberty"
mkdir "output\scripts\ProjectLiberty\Core"
mkdir "output\scripts\ProjectLiberty\Config"
mkdir "output\scripts\ProjectLiberty\Logs"
mkdir "output\scripts\ProjectLiberty\Plugins"

echo Copying Core DLLs...
copy "src\Abstractions\bin\Release\net48\ProjectLiberty.Abstractions.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\Core\bin\Release\net48\ProjectLiberty.Core.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\GameHost\bin\Release\net48\ProjectLiberty.GameHost.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\EventBus\bin\Release\net48\ProjectLiberty.EventBus.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\PluginSystem\bin\Release\net48\ProjectLiberty.PluginSystem.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\Localization\bin\Release\net48\ProjectLiberty.Localization.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\Configuration\bin\Release\net48\ProjectLiberty.Configuration.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\Logging\bin\Release\net48\ProjectLiberty.Logging.dll" "output\scripts\ProjectLiberty\Core\" >nul
copy "src\UI\bin\Release\net48\ProjectLiberty.UI.dll" "output\scripts\ProjectLiberty\Core\" >nul

echo Copying Newtonsoft.Json.dll to Core...
copy "src\LibertyScript\bin\Release\net48\Newtonsoft.Json.dll" "output\scripts\ProjectLiberty\Core\" >nul

if exist "src\LibertyScript\bin\Release\net48\ScriptHookVDotNet3.dll" (
    copy "src\LibertyScript\bin\Release\net48\ScriptHookVDotNet3.dll" "output\scripts\ProjectLiberty\Core\" >nul
)

if exist "src\LibertyScript\bin\Release\net48\LibertyScript.dll" (
    copy "src\LibertyScript\bin\Release\net48\LibertyScript.dll" "output\scripts\" >nul
)

echo Copying plugins...
if exist "Plugins\TestPlugin\bin\Release\net48\TestPlugin.dll" (
    mkdir "output\scripts\ProjectLiberty\Plugins\TestPlugin"
    copy "Plugins\TestPlugin\bin\Release\net48\TestPlugin.dll" "output\scripts\ProjectLiberty\Plugins\TestPlugin\" >nul
)

echo Copying configuration files...
copy "Config\core.json" "output\scripts\ProjectLiberty\Config\" >nul

mkdir "output\scripts\ProjectLiberty\Languages"
mkdir "output\scripts\ProjectLiberty\Languages\en"
mkdir "output\scripts\ProjectLiberty\Languages\ru"
copy "Languages\en\core.json" "output\scripts\ProjectLiberty\Languages\en\" >nul
copy "Languages\ru\core.json" "output\scripts\ProjectLiberty\Languages\ru\" >nul

echo.
echo ========================================
echo DEPLOYMENT INSTRUCTIONS:
echo ========================================
echo.
echo For ScriptHookVDotNet (GTA V):
echo   - Copy all files from output\scripts\ to GTA V\scripts\
echo   - Launch GTA V
echo   - LibertyScript will automatically initialize Project Liberty
echo.
echo For standalone testing:
echo   - Run output\ProjectLiberty.EntryPoint.exe
echo.
echo ========================================

echo Build completed successfully!
echo.
echo All files copied to: output\
echo.
if not defined CI pause
