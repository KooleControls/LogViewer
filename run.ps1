<#
.SYNOPSIS
    Builds and launches LogViewer.

.EXAMPLE
    ./run
    ./run -Configuration Demo
    ./run -NoBuild
    ./run -- --some-app-arg
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release', 'Demo')]
    [string]$Configuration = 'Debug',

    # Skip the build and start whatever is already in bin/.
    [switch]$NoBuild,

    # Build only, don't start the app.
    [switch]$BuildOnly,

    # Everything after `--` is forwarded to LogViewer.exe.
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$AppArgs
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = $PSScriptRoot
$project = Join-Path $root 'LogViewer\LogViewer.csproj'
$framework = 'net8.0-windows7.0'
$exe = Join-Path $root "LogViewer\bin\$Configuration\$framework\LogViewer.exe"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'dotnet SDK not found on PATH. Install the .NET 8 SDK.'
}

if (-not $NoBuild) {
    Write-Host "Building LogViewer ($Configuration)..." -ForegroundColor Cyan
    dotnet build $project -c $Configuration --nologo
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed (exit $LASTEXITCODE)."
    }
}

if ($BuildOnly) {
    Write-Host "Build finished: $exe" -ForegroundColor Green
    exit 0
}

if (-not (Test-Path $exe)) {
    Write-Error "Executable not found: $exe`nRun without -NoBuild to build it first."
}

Write-Host "Starting $exe" -ForegroundColor Cyan
if ($AppArgs) {
    & $exe @AppArgs
}
else {
    & $exe
}
exit $LASTEXITCODE
