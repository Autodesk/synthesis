@echo off
setlocal enabledelayedexpansion
setlocal

where python >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo Python not found. Installing Python...
    curl -o python-installer.exe https://www.python.org/ftp/python/3.11.0/python-3.11.0-amd64.exe
    python-installer.exe /quiet InstallAllUsers=1 PrependPath=1
    del python-installer.exe
)

python -m pip --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo pip not found. Installing pip...
    python -m ensurepip
    python -m pip install --upgrade pip
)

python -m pip install -r requirements.txt

set "EXPORTER_SOURCE_DIR=..\..\..\exporter\SynthesisFusionAddin\"

mkdir tmp\synthesis.bundle\Contents\
xcopy ..\synthesis.bundle .\tmp\synthesis.bundle
xcopy /e /i "%EXPORTER_SOURCE_DIR%" tmp\synthesis.bundle\Contents
tar -a -c -f SynthesisExporter.zip -C tmp synthesis.bundle\*

@REM Find and run pyinstaller, this is a workaround that allows you to call pip packages as scripts without
@REM them being added to the system PATH.
for /f "delims=" %%i in ('pip show pyinstaller') do (
    echo %%i | findstr /b /c:"Location:" >nul
    if not errorlevel 1 (
        set "location_line=%%i"
    )
)

set "executable=!location_line:Location: =!"
for %%a in ("%executable%") do set "executable=%%~dpa"
set "executable=%executable%Scripts\pyinstaller.exe "
set executable=%executable:~0,-1%

%executable% --onefile --add-data "SynthesisExporter.zip;." installer.py

move .\dist\installer.exe .
rmdir /s /q tmp
rmdir /s /q build
rmdir /s /q dist
del SynthesisExporter.zip
del installer.spec

endlocal
