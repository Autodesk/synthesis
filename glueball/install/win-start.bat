@echo off
cd /d "%~dp0"
if not exist ".\certs" mkdir ".\certs"
.\bin\glueball.exe --config-file .\config.toml --cert-dir .\certs