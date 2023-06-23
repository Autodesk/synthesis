@echo off

NET SESSION >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo "Because this script edits the system PATH, it must be run as administrator."
    echo "Please elevate this script's privileges by runing it as administrator."
    pause
    exit /b 1
)

set "protobufVersion=23.3"
set "protobufFolder=protoc-%protobufVersion%-win64"
set "protobufGitDownloadUrl=https://github.com/protocolbuffers/protobuf/releases/download/v%protobufVersion%/%protobufFolder%.zip"
set "dotnetVersion=7"

echo "Downloading protobuf..."
curl -L -o "%protobufFolder%.zip" "%protobufGitDownloadUrl%"

echo "Unzipping protobuf..."
powershell -Command "Expand-Archive -Path %protobufFolder%.zip -DestinationPath '.\%protobufFolder%'"

echo "Copying protobuf to C:/Program Files/..."
move "%protobufFolder%" "C:\Program Files\"

echo "Linking %protobufFolder%/bin to system PATH..."
setx /M PATH "C:\Program Files\%protobufFolder%\bin;%PATH%"

echo "Protobuf installation complete."

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

echo ".NET SDK installation complete."

echo "Cleaning up..."
del "%protobufFolder%.zip"
del dotnet-install.ps1
