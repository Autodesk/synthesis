# FRC Robot Code Emulation

## Overview
The emulator is the Synthesis tool designed to help users test their FRC robot code. Users can upload their own user code as the FRCUserProgram they would normally deploy to the RoboRIO. Its communication with hardware will then be redirected to the engine for simulation and testing.

## Hardware Emulation Layer
The core of Synthesis's emulator is HEL. HEL is a reimplementation of the Ni FPGA which runs on the RoboRIO which runs in an ARM virtual environment rather than the RoboRIO and interfaces with a simulation rather than hardware. This allows robot code to run on users' computers and to communicate with Synthesis. This lower layer of robot code runtime was chosen for re-implementation to enable compatibilty across new releases of WPILib and other external solutions. This is a new system that has been integrated into Synthesis, replacing older versions of the emulator such as the HELBuildTool and prior solutions.
