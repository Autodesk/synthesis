@echo off
copy EngineImporter\\bin\\Debug\\netstandard2.0\\EngineImporter.dll ..\\engine\\Assets\\Packages\\EngineImporter.dll
del /f ..\\engine\\Assets\\Packages\\EngineImporter.dll.meta
