# Synthesis Exporters

## Aardvark-Libraries
Common dependancies for the robot exporters, field exporters, and the Synthesis application.
* CLEW is an OpenCL extension wrangler used in the robot and field exporters to increase export speed.
* ConvexLibraryWrapper is a C# wrapper for providing functionality to generate simplified convex meshes.
* SimulatorAPI is a C# API used for reading and writing BXDF, BXDJ, and BXDA files.
* VHACD is a convex hull decomposition library.

## [Deprecated] BxDFieldExpoter
The Autodesk Inventor addin for exporting fields to Synthesis.

## InventorRobotExporter
The Autodesk Inventor add-in for exporting robot assemblies into Synthesis.

## FusionRobotExporter
The Autodesk Fusion 360 add-in for exporting robot assemblies into Synthesis.

## field_exporter
A standalone application which hooks into Autodesk Inventor to export fields into Synthesis.
Currently, this is only used internally by the Synthesis team.
