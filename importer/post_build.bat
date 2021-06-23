@echo off
copy EngineImporter\\bin\\Debug\\netstandard2.0\\EngineImporter.dll ..\\engine\\Assets\\Packages\\EngineImporter.dll
del /f ..\\engine\\Assets\\Packages\\EngineImporter.dll.meta
copy EngineImporter\\bin\\Debug\\netstandard2.0\\SimulatorAPI.dll ..\\engine\\Assets\\Packages\\SimulatorAPI.dll
del /f ..\\engine\\Assets\\Packages\\SimulatorAPI.dll.meta
