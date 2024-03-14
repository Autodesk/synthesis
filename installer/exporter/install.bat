@echo off

if exist "%AppData%\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis\" (
	echo "Removing existing Synthesis exporter..."
	rmdir "%AppData%\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis\" /Q/S
)

echo "Copying to %AppData%\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis..."
xcopy Synthesis "%AppData%\Autodesk\Autodesk Fusion 360\API\AddIns\Synthesis\" /E
echo "Synthesis Exporter Successfully Installed!"
