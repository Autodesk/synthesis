@echo off

git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..\engine\EngineDeps
dotnet build
call copy_deps.bat
cd ..\..\api
call build.bat
cd ..
