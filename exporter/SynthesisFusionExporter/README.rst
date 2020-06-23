SynthesisFusionExporter
=======================


A Fusion 360 add-in to export assemblies into Synthesis

Usage
-----
The SynthesisFusionExporter add-in is installed into Fusion 360 automatically by the Synthesis installer.

TODO: Robot export instructions

Development Installation (Windows)
----------------------------------

Requirements
^^^^^^^^^^^^
- Python 3.7.6: https://www.python.org/downloads/release/python-376/
- Visual Studio Code: https://code.visualstudio.com/
- Visual Studio Code Python plugin
- Fusion 360
- protoc.exe from the Google Protobuf library (add the folder with the protoc executable to your system path) https://github.com/protocolbuffers/protobuf/releases/latest

Installation
^^^^^^^^^^^^
- git clone https://github.com/Autodesk/synthesis
- Run the compile.bat script in the .../synthesis/exporter/SynthesisFusionExporter/proto folder to generate the protobuf format
- Open Fusion 360
- Navigate to the Tools Tab > Add-ins > Scripts and Add-ins...
- Click the Add-ins tab and click on the green plus
- Navigate to the location of the repo and select the folder .../synthesis/exporter/SynthesisFusionExporter
- Click on SynthesisFusionExporter which will appear in the My Add-ins list
- Click the "Edit" button to open the VSCode editor
- To debug the Add-in, go back to Fusion and click on the dropdown next to the "Run" button, then click "Debug"

Troubleshooting
^^^^^^^^^^^^^^^
Protobuf-related error:

- Restart Fusion 360
- Make sure the protobuf python file has generated in the `proto` subdirectory

Visual studio debugger crashes with socket timeout:

- Open task manager and close all python processes
- If problem persists, uninstall Fusion 360 and reinstall