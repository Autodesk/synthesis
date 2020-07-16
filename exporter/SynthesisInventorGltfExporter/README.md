# SynthesisInventorGltfExporter

## Overview
SynthesisInventorGltfExporter is the Autodesk Inventor add-in for exporting assemblies into Synthesis.

## Building From Source (Windows)

#### Prerequisites:
* Git
* Microsoft Visual Studio 2019
* Autodesk Inventor 2020

#### Setup:
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Import the `SynthesisInventorGltfExporter.sln` Solution in Visual Studio
   - `File -> Open -> Project/Solution`
   - Select `... \synthesis\exporters\SynthesisInventorGltfExporter\SynthesisInventorGltfExporter.sln`
3) Select "Any CPU" next to the "Start" button
4) Right-click on the SynthesisInventorGltfExporter solution in the Solution Explorer and click `Restore NuGet Packages` and wait for restore to complete
5) Build -> Build Solution
