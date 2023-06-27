@echo off

git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..\engine\EngineDeps
call setup.bat
cd ..\..\api
dotnet build
call post_build.bat
cd ..
