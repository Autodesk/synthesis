#!/bin/sh
dotnet build
cp -v Api/bin/Debug/netstandard2.0/Api.dll ../engine/Assets/Packages/Api.dll
rm -f ../engine/Assets/Packages/Api.dll.meta

