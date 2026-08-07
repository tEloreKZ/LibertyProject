[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$cefRoot = Join-Path $root 'third_party\cef'
$buildRoot = Join-Path $root 'Build\Native'
$cmake = 'D:\VisualStudio\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
if (-not (Test-Path $cmake)) { $cmake = 'cmake.exe' }

if (-not (Test-Path (Join-Path $cefRoot 'include\cef_app.h'))) {
    & (Join-Path $PSScriptRoot 'Setup-Cef.ps1') -Destination $cefRoot
}

& $cmake -S (Join-Path $root 'native\ProjectLiberty.Core') -B $buildRoot -A x64 "-DCEF_ROOT=$cefRoot"
if ($LASTEXITCODE -ne 0) { throw 'Native CMake generation failed.' }
& $cmake --build $buildRoot --config $Configuration --target ProjectLibertyCore --parallel
if ($LASTEXITCODE -ne 0) { throw 'Native build failed.' }
Write-Host "Native deployment created at $(Join-Path $buildRoot 'deploy')"
