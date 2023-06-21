#!/bin/sh
cp -v Api/bin/Debug/netstandard2.0/Api.dll ../engine/Assets/Packages/Api.dll
rm -f ../engine/Assets/Packages/Api.dll.meta
cp -v Api/bin/Debug/netstandard2.0/Aardvark.dll ../engine/Assets/Packages/Aardvark.dll
rm -f ../engine/Assets/Packages/Aardvark.dll.meta

