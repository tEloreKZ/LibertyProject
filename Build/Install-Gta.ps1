param(
    [Parameter(Mandatory = $true)]
    [string]$GtaPath,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$deploymentRoot = Join-Path $repositoryRoot 'output\gta-root'
$resolvedGta = [System.IO.Path]::GetFullPath($GtaPath)
$expectedRoot = [System.IO.Path]::GetFullPath($resolvedGta)

if (-not (Test-Path -LiteralPath $resolvedGta -PathType Container)) {
    throw "GTA directory does not exist: $resolvedGta"
}
if (-not (Test-Path -LiteralPath $deploymentRoot -PathType Container)) {
    throw "Deployment is missing. Run build.bat first: $deploymentRoot"
}
if (Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -match '^(GTA5|GTA5_Enhanced|PlayGTAV|ProjectLiberty\.CefSubprocess)$' }) {
    throw 'Close GTA V and Project Liberty CEF subprocesses before installation.'
}

$scriptsRoot = Join-Path $resolvedGta 'scripts'
$projectRoot = Join-Path $scriptsRoot 'ProjectLiberty'
$legacyRoot = Join-Path $resolvedGta 'ProjectLiberty'
New-Item -ItemType Directory -Path $projectRoot -Force | Out-Null
$configPath = Join-Path $projectRoot 'Config\core.json'
$preservedConfig = if (Test-Path -LiteralPath $configPath -PathType Leaf) {
    [System.IO.File]::ReadAllBytes($configPath)
} else { $null }

# Preserve user-created data and any unknown extension folders while consolidating
# the old root folder. Native/UI are replaced by the freshly built package below.
if (Test-Path -LiteralPath $legacyRoot -PathType Container) {
    Get-ChildItem -LiteralPath $legacyRoot -Force |
        Where-Object { $_.Name -notin @('Native', 'ui', 'UI', 'MigrationBackup') } |
        ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination $projectRoot -Recurse -Force
        }
    }

# These directories are fully reproducible and are replaced atomically enough for
# an offline GTA installation. User-owned Saves, Config, Plugins and Logs remain.
foreach ($name in @('Core', 'Native', 'UI', 'Assets', 'Languages')) {
    $generatedPath = [System.IO.Path]::GetFullPath((Join-Path $projectRoot $name))
    if (-not $generatedPath.StartsWith([System.IO.Path]::GetFullPath($projectRoot) + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to clean a directory outside Project Liberty runtime: $generatedPath"
    }
    if (Test-Path -LiteralPath $generatedPath) {
        Remove-Item -LiteralPath $generatedPath -Recurse -Force
    }
}

Get-ChildItem -LiteralPath $deploymentRoot -Force | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $resolvedGta -Recurse -Force
}
if ($null -ne $preservedConfig) {
    [System.IO.File]::WriteAllBytes($configPath, $preservedConfig)
}

$required = @(
    (Join-Path $resolvedGta 'ProjectLiberty.Core.asi'),
    (Join-Path $scriptsRoot 'LibertyScript.dll'),
    (Join-Path $projectRoot 'Native\ProjectLiberty.CefSubprocess.exe'),
    (Join-Path $projectRoot 'UI\index.html')
)
foreach ($path in $required) {
    if (-not (Test-Path -LiteralPath $path)) { throw "Installation verification failed: $path" }
}

if (Test-Path -LiteralPath $legacyRoot -PathType Container) {
    $legacyResolved = [System.IO.Path]::GetFullPath($legacyRoot)
    if (-not $legacyResolved.StartsWith($expectedRoot + [System.IO.Path]::DirectorySeparatorChar,
        [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a directory outside GTA root: $legacyResolved"
    }
    Remove-Item -LiteralPath $legacyResolved -Recurse -Force
}

Write-Host 'Project Liberty installed successfully.' -ForegroundColor Green
Write-Host "ASI loader: $resolvedGta\ProjectLiberty.Core.asi"
Write-Host "Managed entry: $scriptsRoot\LibertyScript.dll"
Write-Host "Unified runtime: $projectRoot"
