# FusionRobotExporter
Autodesk Fusion 360 Robot exporter using the Fusion 360 API for C++.

## Currently builds on:
*  Windows (VS17)

### To build this addin you need a couple of dependencies:
1.  Fusion 360 (Up to date)
2.  Visual Studio Community 2015+

### Steps to build and debug (first time):
1.  Install Fusion 360
2.  Open solution (`FusionSynth.sln`) as administrator
3.  Change Debug command in project settings to Fusion360.exe (usually located in "AppData\Local\Autodesk\webdeploy\production\<varies>")
4.  Build the solution
5.  Start the debugger
6.  In Fusion 360, open the Add-Ins menu, select the Add-Ins tab, and click the ➕ icon next to "My Add-Ins"
7.  Select the "FusionExporter" folder
8.  Close Fusion 360 and start debugging again

## Documentation
*  [Wiki Page](https://github.com/Autodesk/synthesis/wiki/Fusion-Exporter)
*  Doxygen Page (Coming Soon)

## Autodesk Fusion 360 API:
*  [Documentation](https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-7B5A90C8-E94C-48DA-B16B-430729B734DC)
*  [Object Model](https://help.autodesk.com/cloudhelp/ENU/Fusion-360-API/images/Fusion.pdf)
