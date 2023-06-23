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

echo "Installing .NET SDK..."
winget install Microsoft.DotNet.SDK.%dotnetVersion% --accept-source-agreements

echo "Cleaning up..."
del "%protobufFolder%.zip"
