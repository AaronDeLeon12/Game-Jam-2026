param(
    [string]$Version = "0.1.0-beta"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$buildName = "TalesOfIvoryMoss_Beta_$($Version)_Windows"
$buildFolder = Join-Path $projectRoot "Builds\TalesOfIvoryMoss_Beta_0.1.0_Windows"
$zipPath = Join-Path $projectRoot "Builds\$buildName.zip"
$exePath = Join-Path $buildFolder "TalesOfIvoryMoss.exe"

if (!(Test-Path $exePath)) {
    throw "Build executable not found at $exePath. Run Unity menu Build/Beta/Windows x64 first."
}

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $buildFolder "*") -DestinationPath $zipPath -Force
Write-Host "Created beta zip: $zipPath"
