@echo off
echo "Linking LLVM to system PATH..."

NET SESSION >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo "Because this script edits the system PATH, it must be run as administrator."
    echo "Please elevate this script's privileges and try again."
    pause
    exit /b
)

setx /m PATH "C:\Program Files\LLVM\bin;%PATH%"
echo "Done!"
pause
