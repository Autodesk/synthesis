@echo off

if exist Synthesis\ (
	rmdir Synthesis /Q/S
	echo Removed .\Synthesis
)

xcopy ..\..\exporter\SynthesisFusionAddin Synthesis\ /E
echo Copied exporter into .\Synthesis
