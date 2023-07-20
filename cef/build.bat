@echo off

if not exist "./build" mkdir "./build"
cd build
cmake -G "Unix Makefiles" -DPROJECT_ARCH="x86_64" ../
make
