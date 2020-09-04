# SynthesisFusionGltfExporter
A cross-platform Autodesk® Fusion 360™ add-in to export assemblies into the glTF file format.
Synthesis-specific data like mechanical joints and physics properties are serialized in the exported glTF files using the “extras” tag, a feature in the glTF spec for including miscellaneous data. 

## Usage
The SynthesisFusionGltfExporter add-in is installed into Fusion 360 automatically by the Synthesis installer.

The add-in should run automatically, adding a tab labeled "Synthesis glTF Exporter" after an assembly has been opened.
If not, here's how to start the add-in manually:
- Navigate to the "Tools" menu in the Design workspace.
- Click on the "Add-Ins" button.
- In the window that appears, navigate to the "Add-Ins" tab.
- Click on SynthesisFusionGltfExporter in the list under "My Add-Ins".
- Click the "Run" button with the green arrow.
- On the first time the add-in is launched, it will attempt to install the python dependencies, which will take ~30 seconds. If the installation fails, check your internet connection and try again.
- If the installation succeeds, a new tab will appear in the Design workspace named "Synthesis Gltf Exporter".

Using the add-in:
- Open the design you want to export.
- Navigate to the tab labeled "Synthesis Gltf Exporter".
- Click the button labeled "Export to Gltf".

## Development Installation (Windows)

### Getting Started

See the [wiki](https://github.com/Autodesk/synthesis/wiki/fus-gltf-overview) for an overview of the exporter and links to relevant documentation.

### Requirements

- Visual Studio Code: https://code.visualstudio.com/
- Visual Studio Code Python plugin
- Fusion 360
- protoc.exe from the Google Protobuf library (add the folder with the protoc executable to your system path) https://github.com/protocolbuffers/protobuf/releases/latest

_Optional_

- Jetbrains PyCharm
- Fusion 360 Scripting plugin for PyCharm https://plugins.jetbrains.com/plugin/11343-fusion-360-scripting

### Installation

- `git clone https://github.com/Autodesk/synthesis --recursive`
- Open Fusion 360
- Navigate to the Tools Tab > Add-ins > Scripts and Add-ins...
- Click the Add-ins tab and click on the green plus
- Navigate to the location of the repo and select the folder `.../synthesis/exporter/SynthesisFusionGltfExporter`
- Click on SynthesisFusionGltfExporter which will appear in the My Add-ins list
- Click the "Edit" button to open the VSCode editor
- To debug the Add-in, go back to Fusion and click on the dropdown next to the "Run" button, then click "Debug"

### Releasing
- Open Fusion and test the add-in functionality.
- Zip up the project directory `SynthesisInventorGltfExporter`

#### Installer (Windows)
- The installer should place all the files in zipped release folder at `%AppData%\Autodesk\Autodesk Fusion 360\API\AddIns\SynthesisInventorGltfExporter`

#### Installer (Macos)
- The installer should place all the files in zipped release folder at `$HOME/Library/Application Support/Autodesk/Autodesk Fusion 360/API/AddIns/SynthesisInventorGltfExporter`

### Troubleshooting
Protobuf-related error:

- Restart Fusion 360
- Make sure the protobuf python file has generated in the `gltf/extras/proto` subdirectory

Visual studio debugger crashes with socket timeout:

- Open task manager and close all python processes
- If problem persists, uninstall Fusion 360 and reinstall