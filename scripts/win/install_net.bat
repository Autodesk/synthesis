@echo off

REM Check if .NET SDK is already installed
dotnet --version >nul 2>&1
if %errorlevel% equ 0 (
    echo ".NET SDK is already installed."
) else (
    echo "Installing .NET SDK..."
    PowerShell -Command "Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; .\dotnet-install.ps1 -Channel LTS -InstallDir 'C:\Program Files\dotnet' -NoPath"

    echo "Adding .NET SDK to PATH..."
    setx /m PATH "%PATH%;C:\Program Files\dotnet"

    echo "Cleaning up..."
    del dotnet-install.ps1
)

echo ".NET SDK installation complete."
