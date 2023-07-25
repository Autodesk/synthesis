@echo off
mkdir build
cd build
cmake -G "Ninja" ../
ninja
