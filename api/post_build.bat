@echo off
copy Api\\bin\\Debug\\netstandard2.0\\Api.dll ..\\engine\\Assets\\Packages\\Api.dll
del /f ..\\engine\\Assets\\Packages\\Api.dll.meta
