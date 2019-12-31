# InventorRobotExporter

## Overview
InventorRobotExporter is the Autodesk Inventor add-in for exporting robot assemblies into Synthesis.

## Building From Source (Windows)

#### Prerequisites:
* Git
* Microsoft Visual Studio 2019
* Autodesk Inventor 2020

#### Setup:
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Import the `InventorRobotExporter.sln` Solution in Visual Studio
   - `File -> Open -> Project/Solution`
   - Select `... \synthesis\exporters\InventorRobotExporter\InventorRobotExporter.sln`
3) Select "Any CPU" next to the "Start" button
4) Right-click on the InventorRobotExporter solution in the Solution Explorer and click `Restore NuGet Packages` and wait for restore to complete
5) Build -> Build Solution

## Development Documentation
*  [Wiki Page](https://github.com/Autodesk/synthesis/wiki/Inventor-Robot-Exporter)

## File Structure
```
InventorRobotExporter
|
├── InventorRobotExporter - The Inventor add-in Visual Studio project
└── RobotExporterTests - Visual Studio project containing unit tests for the Inventor add-in
```
