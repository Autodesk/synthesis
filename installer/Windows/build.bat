@echo off
setlocal enabledelayedexpansion
setlocal

set "EXPORTER_SOURCE_DIR=..\..\exporter\SynthesisFusionAddin\"

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
