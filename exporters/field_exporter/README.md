# field_exporter

## Road Map
* TBD

## Requirements
* Git
* Microsoft Visual Studio 2019
* Autodesk Inventor 2019

## How to Build
1) Clone the repository by running `git clone --recursive https://github.com/Autodesk/synthesis`
2) Launch Visual Studio in Admin Mode for necessary permissions
3) Import the FieldExporter.sln Solution in Visual Studio
   - File -> Open -> Project/Solution
   - Select `... \synthesis\exporters\field_exporter\FieldExporter.sln`
4) Select "Any CPU" next to the "Start" button
5) Build -> Build Solution

If you get a NewtonSoft Error: 1) right click the solution and select `Manage Nuget Packages` 2) make sure NewtonSoft Json appears, if not download it via nuget and apply it to the Field Exporter 3) click the blue update arrow

## Current Issues
* Only used internally for the Synthesis team to export new fields
