@echo off

for /f "usebackq delims=" %%G in (`powershell -Command "Get-ExecutionPolicy"`) do set "executionPolicy=%%G"

if /i "%executionPolicy%" neq "RemoteSigned" (
    echo "Current Execution Policy is not RemoteSigned. This is required for the .NET installation."
    echo "Setting Execution Policy to RemoteSigned..."
    powershell -Command "Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned"
    echo "Execution Policy set to RemoteSigned."
)

echo "Installing .NET SDK..."
PowerShell -Command "Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1'; .\dotnet-install.ps1 -Channel LTS -InstallDir 'C:\Program Files\dotnet' -NoPath"

echo "Adding .NET SDK to PATH..."
setx /m PATH "%PATH%;C:\Program Files\dotnet"

echo "Cleaning up..."
del dotnet-install.ps1

echo ".NET SDK installation complete."
