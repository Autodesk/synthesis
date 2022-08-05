# Inventor Mirabuf Exporter
An Autodesk® Inventor™ addin for exporting assemblies into the [Mirabuf](https://www.mirabuf.dev/) protocol format used by Synthesis.

## Requirements
- Microsoft Visual Studio 2019+
- Autodesk Inventor 2020+
- Google Protobuf Compiler [3.17.3](https://github.com/protocolbuffers/protobuf/releases/tag/v3.17.3)

## Building
- Install the Inventor `DeveloperTools.msi` located at `C:\Users\Public\Documents\Autodesk\Inventor <version>\SDK` [(more info)](https://help.autodesk.com/view/INVNTOR/2020/ENU/?guid=GUID-6FD7AA08-1E43-43FC-971B-5F20E56C8846)
- `git clone --recurse-submodules https://github.com/Autodesk/synthesis`
- `protoc -I=. --csharp_out=./synthesis/exporter/InventorMirabufExporter/Mirabuf ./synthesis/mirabuf/*.proto`
- Open the solution as an Administrator in Visual Studio
- Ensure the run configuration is set to the correct path of your Inventor installation. The default is `C:/Program Files/Autodesk/Inventor 2020/Bin/Inventor.exe`
- Build > Build Solution, then Run or Debug the solution using the `Debug | Any CPU` configuration.

## Packaging
- Delete all files at `%AppData%\Autodesk\ApplicationPlugins\InventorMirabufExporter`
- Clean and build the solution with the `Release | Any CPU` configuration.
- Open Inventor and test the add-in functionality.
- Zip the contents of the folder at `%AppData%\Autodesk\ApplicationPlugins\InventorMirabufExporter`
- NSIS installer should extract the contents of the zip folder to `%AppData%\Autodesk\ApplicationPlugins\InventorMirabufExporter`
