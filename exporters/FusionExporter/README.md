# FusionExporter (FusionSynth)

## Road Map
* TBD

## Requirements
* Git
* Autodesk Fusion 360 2019
* Microsoft Visual Studio 2019

## Setup Instructions
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Launch Visual Studio in Admin Mode for necessary permissions
3) Import the FusionSynth.sln Solution in Visual Studio
   - File -> Open -> Project/Solution
   - Select `... \synthesis\exporters\FusionExporter\FusionSynth.sln`
4) Change Debug command in project settings to Fusion360.exe
    (Usually located in "AppData\Local\Autodesk\webdeploy\production\<varies>")
5) Build > Build Solution
6) Start the debugger
7) In Fusion 360, open the Add-Ins menu, select the Add-Ins tab, and click the ➕ icon next to "My Add-Ins"
8) Select the "FusionExporter" folder
9) Close Fusion 360 and start debugging again

## Current Issues
* TBD

## Documentation
*  [Wiki Page](https://github.com/Autodesk/synthesis/wiki/Fusion-Exporter)
*  Doxygen Page (Coming Soon)

### File Structure
```
FusionExporter
|
├── Include - Libraries used in FusionSynth
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
## Autodesk Fusion 360 API:
*  [Documentation](https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-7B5A90C8-E94C-48DA-B16B-430729B734DC)
*  [Object Model](https://help.autodesk.com/cloudhelp/ENU/Fusion-360-API/images/Fusion.pdf)
