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
This is the main Inventor exporter that is bundles with Synthesis. It is broken into a few subprojects:
* InventorRobotExporter is the Inventor addin that interacts directly with the Inventor GUI.
* packages are external libs that the exporter uses to simplify meshes.
* robot_exporter is the legacy Inventor plugin tha the BxDRobotExpoter uses for backend robot information and some GUI elements.

## FusionExporter
This is the new Fusion 360 Exporter

## field_exporter
This is the current field exporter, currently it is only used internally for the Synthesis team to export new fields

## robot_exporter
This is the old robot exporter, it has been deprecated in favor of the InventorRobotExporter. It is a plugin that can hook into the Inventor Instance and read the model data from Inventor to allow users to export their robots
