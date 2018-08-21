# FusionExporter (FusionSynth)
Autodesk Fusion 360 Robot exporter using the Fusion 360 API for C++.

## Requirements
*  Windows
*  Fusion 360 (Up to date)
*  Visual Studio Community 2015+

## Setup Instructions
1.  Install Fusion 360
2.  Open solution (`FusionSynth.sln`) as administrator
3.  Change Debug command in project settings to Fusion360.exe  
    (Usually located in "AppData\Local\Autodesk\webdeploy\production\<varies>")
4.  Build the solution
5.  Start the debugger
6.  In Fusion 360, open the Add-Ins menu, select the Add-Ins tab, and click the ➕ icon next to "My Add-Ins"
7.  Select the "FusionExporter" folder
8.  Close Fusion 360 and start debugging again

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
