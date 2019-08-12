# Exporters File Structure

## Aardvark-Libraries
This repository consists of several libraries containing shared functionality of the robot and field exporters and the simulator itself.
* CLEW is an OpenCL extension wrangler used in the robot and field exporters to increase export speed.
* ConvexLibraryWrapper is a C# wrapper for providing functionality to generate simplified convex meshes.
* SimulatorAPI is a C# API used for importing adn exporting BXDF, BXDJ, and BXDA files.
* VHACD is a convex hull decomposition library.

## BxDFieldExpoter
This is the old Inventor addin that was used to exporter fields to Synthesis, this has since been depracated

## InventorRobotExporter
This is the main Inventor exporter that is bundled with Synthesis. It is broken into a few subprojects:
* InventorRobotExporter is the Inventor addin
* RobotExporterTests is the project with unit tests for the exporter addin

## FusionRobotExporter
This is the Fusion 360 Robot Exporter

## field_exporter
This is the current field exporter, currently it is only used internally for the Synthesis team to export new fields
