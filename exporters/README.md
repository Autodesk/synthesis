# Synthesis Exporters

The Synthesis exporters process CAD models from Inventor and Fusion 360 in order to export them to the Synthesis simulator. 

## BxDFieldExporter

This exporter is for getting models of fields into Synthesis using an Inventor application addin. However, due to limitations with Inventor API and the scope of development this version of the field exporter is deprecated, and is no longer available to users. 

## BxDRobotExporter

This exporter is the most up-to-date robot exporter, and is available to users through our current release of Synthesis (4.2.0.1 at the time that this README was created). It features an Inventor application addin that allows users to seemlessly export their model to Synthesis.

##FusionExporter

This exporter is similar to BxDRobot, with the exception that it is an addin for Fusion 360 instead of Inventor.

## field_exporter

This is the legacy version of the field exporter, which features an application for exporting that connects to an instance of Inventor. A version of this project is under development for internal use only, in order to allow for jointed field elements in the Synthesis simulator. 

## robot_exporter

This is the legacy version of the robot exporter, which the BxDRobotExporter project was based on. Similar to field_exporter, it is a standalone application that is no longer available to users. 