# FusionRobotExporter

## Overview
FusionRobotExporter is the Autodesk Fusion 360 add-in for exporting robot assemblies into Synthesis.

## Building From Source (Windows)
#### Prerequisites:
* git
* vcpkg (https://github.com/microsoft/vcpkg)
* cpprestsdk (https://github.com/microsoft/cpprestsdk)
* Autodesk Fusion 360
* Microsoft Visual Studio 2019

#### Setup:
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Launch Visual Studio as an Administrator
3) Open FusionRobotExporter.sln in Visual Studio
   - File -> Open -> Project/Solution
   - Select `...\synthesis\exporters\FusionRobotExporter\FusionRobotExporter.sln`
4) Change Debug command in project settings to the Fusion 360 executable
   - `Debug > FusionRobotExporter Properties... > Configuration Properties > Debugging > Command`
   - Change the value to your Fusion360 executable `C:\Users\<your_username>\AppData\Local\Autodesk\webdeploy\production\<varies>\Fusion360.exe`
5) Start debugging the solution by navigating to `Debug > Start Debugging`
6) In Fusion 360, navigate to the `Tools` tab in the `Design` environment, open the `Add-Ins` manager, select the `Add-Ins` tab, and click the `+` icon next to `My Add-Ins`
7) Navigate to the cloned repository and open the folder under `...\synthesis\exporters\FusionRobotExporter\Debug`
8) Close Fusion 360 and start debugging again in Visual Studio

## Documentation
*  [Wiki Page](https://github.com/Autodesk/synthesis/wiki/Fusion-Exporter)

## File Structure
```
FusionRobotExporter
|
├── Include - Libraries used in the FusionRobotExporter
|   └── rapidjson - JSON library. Used for saving configurations for later use
|
├── Resources - Images used in the Fusion add-in
|   └── SynthesisIcons - Icons used for add-in buttons
|
├── palette - HTML pages and JavaScript used in the add-in's palettes
|   ├── css - Global styling used in palette pages
|   ├── js - Scripts used for receiving and sending information to/from palettes
|   └── resources - Fonts used in palettes
|
└── Source - C++ source code (see wiki page for more info)
    ├── AddIn - Manages add-in user interface
    └── Data - Stores robot configuration and writing to BXDA and BXDJ files
```
