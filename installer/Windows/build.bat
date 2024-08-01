@echo off
setlocal enabledelayedexpansion
setlocal

set "FUSION_ADDIN_LOCATION=%USERPROFILE%\AppData\Local\Autodesk\Autodesk Fusion 360\API\AddIns\"
set "EXPORTER_SOURCE_DIR=..\..\exporter\SynthesisFusionAddin\"

mkdir tmp\
xcopy /e /i "%EXPORTER_SOURCE_DIR%" tmp\
tar -a -c -f SynthesisExporter.zip tmp\*

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

rmdir /s /q tmp
del SynthesisExporter.zip

endlocal
