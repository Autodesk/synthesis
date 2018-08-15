# FRC Robot Code Emulation

## Overview
The emulator is the Synthesis tool designed to help users test their FRC robot code ([Getting started with FRC control system](https://wpilib.screenstepslive.com/s/currentCS "FRC Control System")). Users can upload their own user code as the FRCUserProgram they would normally deploy to the RoboRIO. Its communication with hardware will then be redirected to the [engine](../engine "Engine Source") for simulation and testing.

## Hardware Emulation Layer
The core of Synthesis's emulator is HEL. HEL is a reimplementation of the Ni FPGA which runs on the RoboRIO which runs in an [ARM virtual environment](./emulator-building.md "Building the Emulator") rather than the RoboRIO and interfaces with a simulation rather than hardware. This allows robot code to run on users' computers and to communicate with Synthesis. This lower layer of robot code runtime was chosen for re-implementation to enable compatibilty across new releases of WPILib and other external solutions. This is a new system that has been integrated into Synthesis, replacing older versions of the emulator such as the HELBuildTool and prior solutions. Read more [here](./hel/README.md "HEL README").

## Engine and Exporter Integration

The emulator communicates with the engine over TCP sockets, serializing and deserializing data as JSON. Although HEL may be able to support emulation of a given input or output, it is only valuable if it can interface with the simulation, which can supply the user with feedback on their code and robot designs. Additonally, the engine uses the robot model exported using Synthesis's [robot exporter tool](../exporters/robot_exporter "Robot Exporter Source") for its own simulation and generation and implementation of data used in communication with HEL. In terms of emulation, the robot exporter tool is used to specify which outputs and inputs are attached to which parts of the robot. So as features are added to HEL to interface with robot code, features must also be added to the engine and robot exporter to interface with HEL. 

Also, note that currently, the emulator must be installed as part of a separate package from the main Synthesis install. 
