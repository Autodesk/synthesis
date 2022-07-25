@echo off
copy ServerApi\\bin\\Debug\\netstandard2.0\\ServerApi.dll ..\\engine\\Assets\\Packages\\ServerApi.dll
del /f ..\\engine\\Assets\\Packages\\ServerApi.dll.meta
