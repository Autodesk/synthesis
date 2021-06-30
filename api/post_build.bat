@echo off
copy Api\\bin\\Debug\\netstandard2.0\\Api.dll ..\\engine\\Assets\\Packages\\Api.dll
del /f ..\\engine\\Assets\\Packages\\Api.dll.meta
copy Api\\bin\\Debug\\netstandard2.0\\Aardvark.dll ..\\engine\\Assets\\Packages\\Aardvark.dll
del /f ..\\engine\\Assets\\Packages\\Aardvark.dll.meta
