param (
    [string]$RustProjectPath = "./lib/clipthat",
    [string]$OutputPath = "./pkg/ClipThat",
    [string]$DllName = 'clipthat.dll'
)

$ErrorActionPreference = 'Stop'

Write-Host "Building Rust project..."
Push-Location $RustProjectPath

try {
    cargo build --release
} catch {
    Write-Host "Rust build failed!"
    Pop-Location
    exit 1
}

$BuildOutput = "./target/release/$DllName"
if (-Not (Test-Path $BuildOutput)) {
    Write-Host "Build succeeded, but DLL not found at $BuildOutput"
    Pop-Location
    exit 1
}

if (-Not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force
}

Write-Host "Copying $DllName to $OutputPath..."
Copy-Item $BuildOutput "$OutputPath/$DllName" -Force

Pop-Location

Write-Host "Build and copy completed successfully."