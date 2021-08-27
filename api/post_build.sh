#!/bin/sh
cp Api/bin/Debug/netstandard2.0/Api.dll ../engine/Assets/Packages/Api.dll
rm -f ../engine/Assets/Packages/Api.dll.meta
cp Api/bin/Debug/netstandard2.0/SimulatorAPI.dll ../engine/Assets/Packages/SimulatorAPI.dll
rm -f ../engine/Assets/Packages/SimulatorAPI.dll.meta

