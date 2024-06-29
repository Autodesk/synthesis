@echo off

git submodule update --init --recursive
cd protocols
call proto_compile.bat
cd ..
