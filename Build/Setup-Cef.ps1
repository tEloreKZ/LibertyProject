[CmdletBinding()]
param(
    [string]$Destination = (Join-Path $PSScriptRoot '..\third_party\cef')
)

$ErrorActionPreference = 'Stop'
$version = '150.0.10+g8042e43+chromium-150.0.7871.101'
$archiveName = "cef_binary_${version}_windows64_minimal.tar.bz2"
$urlName = $archiveName.Replace('+', '%2B')
$thirdParty = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\third_party'))
$destinationPath = [IO.Path]::GetFullPath($Destination)
$archivePath = Join-Path $thirdParty 'cef_windows64_minimal.tar.bz2'

if (Test-Path (Join-Path $destinationPath 'include\cef_app.h')) {
    Write-Host "CEF already exists at $destinationPath"
    exit 0
}

New-Item -ItemType Directory -Path $thirdParty -Force | Out-Null
Write-Host "Downloading CEF $version..."
& curl.exe -L --fail --retry 3 -o $archivePath "https://cef-builds.spotifycdn.com/$urlName"
if ($LASTEXITCODE -ne 0) { throw 'CEF download failed.' }

Write-Host 'Extracting CEF...'
& tar.exe -xf $archivePath -C $thirdParty
if ($LASTEXITCODE -ne 0) { throw 'CEF extraction failed.' }
$extracted = Get-ChildItem $thirdParty -Directory -Filter "cef_binary_${version}_windows64_minimal" | Select-Object -First 1
if (-not $extracted) { throw 'The expected CEF directory was not extracted.' }
Move-Item -LiteralPath $extracted.FullName -Destination $destinationPath
Write-Host "CEF installed at $destinationPath"

