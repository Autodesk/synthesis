@echo off
setlocal enabledelayedexpansion
setlocal

set "FUSION_ADDIN_LOCATION=%USERPROFILE%\AppData\Local\Autodesk\Autodesk Fusion 360\API\AddIns\"
set "EXPORTER_SOURCE_DIR=..\..\exporter\SynthesisFusionAddin\"

mkdir tmp\
xcopy /e /i "%EXPORTER_SOURCE_DIR%" tmp\
@REM pushd tmp\
tar -a -c -f resources.zip tmp\*
@REM popd

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

%executable% --onefile --add-data "resources.zip;." installer.py

rmdir /s /q tmp
del resources.zip

endlocal
