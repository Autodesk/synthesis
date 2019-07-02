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
   - `File > Open > Project/Solution`
   - Select `...\synthesis\exporters\FusionExporter\FusionSynth.sln`
4) Add your Fusion360 executable as the target for debugging
   - In Visual Studio, navigate to `Project > FusionSynth Properties...`
   - In the menu on the left, navigate to `Configuration Properties > Debugging`
   - Change the `Command` property in the main menu to your Fusion360 executable (Usually located in `C:\Users\<your username>\AppData\Local\Autodesk\webdeploy\production\<long random string>\Fusion360.exe`)
5) Build the solution (`Build > Build Solution`)
6) Start the debugger
7) Add the Add-In to Fusion 360
   - In Fusion 360, click the button labeled `Add-Ins`
   - In the popup, select the `Add-Ins` tab
   - Click the ➕ icon next to `My Add-Ins`
   - Navigate to the `FusionExporter` project folder (`...\synthesis\exporters\FusionExporter\`)
   - Click on the add-in named `FusionSynth`, which should appear under the My Add-Ins folder (if not, rebuild the project in visual studio)
   - Ensure the `Run on Startup` box is checked
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
