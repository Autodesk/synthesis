# SynthesisInventorGltfExporter
An Inventor add-in to export assemblies into the glTF file format.
Synthesis-specific data like mechanical joints and physics properties are serialized in the exported glTF files using the “extras” tag, a feature in the glTF spec for including miscellaneous data. 

## Usage
The SynthesisInventorGltfExporter add-in is installed into Inventor automatically by the Synthesis installer.

The add-in should run automatically, adding a tab labeled "Synthesis glTF Exporter" when an assembly is opened.
If not, here's how to start the add-in manually:
- Open any assembly document.
- Navigate to the "Environments" tab.
- Click on the "Add-Ins" button.
- Click on "Synthesis GLTF Exporter" in the list under the "Applications" tab.
- Ensure the "Loaded/Unloaded" and "Load Automatically" checkboxes are checked in the "Load Behavior" section.

Using the add-in:
- Open the assembly you want to export.
- Navigate to the tab labeled "Synthesis glTF Exporter".
- Click the button labeled "Export Gltf".

## Development Installation (Windows)

### Requirements
- Visual Studio 2019 or Jetbrains Rider
- Inventor 2020+
- protoc.exe from the Google Protobuf library (add the folder with the protoc executable to your system path) https://github.com/protocolbuffers/protobuf/releases/latest

### Installation
- Install the inventor DeveloperTools.msi and UserTools.msi at `C:\Users\Public\Documents\Autodesk\Inventor <version>\SDK`. (more info: http://help.autodesk.com/view/INVNTOR/2019/ENU/?guid=GUID-6FD7AA08-1E43-43FC-971B-5F20E56C8846)
- `git clone https://github.com/Autodesk/synthesis --recursive`
- Open the solution as an Administrator in Visual Studio 2019 or Jetbrains Rider.
- Ensure the run configuration has the correct path to your Inventor installation. The default is `C:/Program Files/Autodesk/Inventor 2020/Bin/Inventor.exe`
- Create the directory `C:\Program Files\Autodesk\Synthesis\SynthesisInventorGltfExporter` if it doesn't exist.
- Build > Clean, then Run or Debug the solution using the `Debug | Any CPU` configuration.

## Troubleshooting
Changes to the exporter code aren't taking effect / The exporter add-in won't start:
- The post-build event copies the built add-in libraries to `C:\Program Files\Autodesk\Synthesis\SynthesisInventorGltfExporter`, but if the files are in use, this step will fail.
- Close all running instances of Inventor
- Delete all of the files at `C:\Program Files\Autodesk\Synthesis\SynthesisInventorGltfExporter`
- Clean and build the solution, and ensure the built files are getting copied to `C:\Program Files\Autodesk\Synthesis\SynthesisInventorGltfExporter`
- Ensure the `Autodesk.SynthesisInventorGltfExporter.Inventor.addin` file is getting copied to `%AppData%\Autodesk\ApplicationPlugins\SynthesisInventorGltfExporter`

Visual Studio shows a "System.NullReferenceException: 'Object variable or With block variable not set'" and/or "Microsoft.VisualBasic.pdb not loaded":
- Cause is currently unknown, but the exporter seems to work anyways if you uncheck "Break when this exception type is thrown" on the exception window and press continue.