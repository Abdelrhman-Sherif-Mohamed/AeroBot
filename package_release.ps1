# AeroBot Release Packaging Script
param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$buildDir = Join-Path $root "Build"
$releaseDir = Join-Path $root "Releases"
$zipFile = Join-Path $releaseDir "AeroBot-v$Version.zip"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   AeroBot v$Version Release Packager   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (-not (Test-Path $buildDir)) {
    Write-Error "Build directory does not exist! Run dotnet build first."
}

if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}

if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

# Create a clean temp staging folder
$staging = Join-Path $env:TEMP "AeroBot_Staging_$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $staging | Out-Null

try {
    Write-Host "1. Copying Build files to clean staging directory..." -ForegroundColor Yellow
    Copy-Item -Path "$buildDir\*" -Destination $staging -Recurse -Force

    Write-Host "2. Removing user cache, personal logs, and debug symbols..." -ForegroundColor Yellow
    $removePatterns = @(
        (Join-Path $staging "User"),
        (Join-Path $staging "Data\Logs"),
        (Join-Path $staging "*.pdb"),
        (Join-Path $staging "*.log"),
        (Join-Path $staging "boot-error.log"),
        (Join-Path $staging "rsbot_download_temp")
    )
    foreach ($pat in $removePatterns) {
        if (Test-Path $pat) {
            Remove-Item $pat -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    # Ensure empty Data/Logs and User directories exist
    New-Item -ItemType Directory -Path (Join-Path $staging "Data\Logs") -Force | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $staging "User") -Force | Out-Null

    # Copy version.json and README.md into staging root as reference
    Copy-Item -Path (Join-Path $root "version.json") -Destination $staging -Force -ErrorAction SilentlyContinue
    Copy-Item -Path (Join-Path $root "README.md") -Destination $staging -Force -ErrorAction SilentlyContinue

    Write-Host "3. Compressing package into: $zipFile ..." -ForegroundColor Yellow
    Compress-Archive -Path "$staging\*" -DestinationPath $zipFile -CompressionLevel Optimal

    $zipInfo = Get-Item $zipFile
    $zipMb = [Math]::Round($zipInfo.Length / 1MB, 2)

    Write-Host ""
    Write-Host "--------------------------------------------------------" -ForegroundColor Green
    Write-Host " SUCCESS! Release package created successfully!" -ForegroundColor Green
    Write-Host " File: $zipFile ($zipMb MB)" -ForegroundColor Green
    Write-Host "--------------------------------------------------------" -ForegroundColor Green
    Write-Host ""
    Write-Host "Steps to publish on GitHub:" -ForegroundColor White
    Write-Host "1. Go to https://github.com/Abdelrhman-Sherif-Mohamed/AeroBot/releases/new" -ForegroundColor Gray
    Write-Host "2. Tag: v$Version" -ForegroundColor Gray
    Write-Host "3. Title: AeroBot v$Version" -ForegroundColor Gray
    Write-Host "4. Upload: $zipFile" -ForegroundColor Gray
    Write-Host "5. Click 'Publish release'" -ForegroundColor Gray
}
finally {
    if (Test-Path $staging) {
        Remove-Item $staging -Recurse -Force -ErrorAction SilentlyContinue
    }
}
