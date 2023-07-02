@echo off

git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..\engine\EngineDeps
call setup.bat
cd ..\..\api
call build.bat
cd ..
