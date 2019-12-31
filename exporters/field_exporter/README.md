# field_exporter

## Road Map
* TBD

## Requirements
* Git
* Microsoft Visual Studio 2019
* Autodesk Inventor 2020

## How to Build
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Launch Visual Studio in Admin Mode for necessary permissions
3) Open the FieldExporter.sln solution in Visual Studio
   - File -> Open -> Project/Solution
   - Select `... \synthesis\exporters\field_exporter\FieldExporter.sln`
4) Select "Any CPU" next to the "Start" button
5) Right-click on the FieldExporter solution in the Solution Explorer and click `Restore NuGet Packages` and wait for restore to complete
6) Build -> Build Solution

**Note about debugging FieldExporter:**
If visual studio is run as an administrator (necessary for copying the executable to `Program Files` in the post-build task) the FieldExporter, when launched in debug mode from visual studio, can't attach to the Inventor process which was not launched as administrator.
If visual studio is launched normally and the post-build task is removed, the exporter is able to attach to Inventor.
The current post-build task, which copies the built executable to the Synthesis folder in `Program Files`, seems unnececary and should probably be removed in the future so visual studio does not need to be run as administrator to edit the solution.

## Current Issues
* Only used internally for the Synthesis team to export new fields
