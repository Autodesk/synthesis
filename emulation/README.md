# FRC Robot Code Emulation

## Overview
The emulator is the Synthesis tool designed to help users test their FRC robot code ([Getting started with FRC control system](https://wpilib.screenstepslive.com/s/currentCS "FRC Control System")). Users can upload their own user code as the FRC user programs they would normally deploy to the RoboRIO. Its normal communication with hardware is then redirected to the [engine](../engine "Engine Source") for simulation and testing.

## HEL - Hardware Emulation Layer
The core of Synthesis's emulator is HEL. HEL is a reimplementation of the NI FPGA, which normally runs on the RoboRIO, that instead runs in an [ARM](./emulator_building.md "Building the Emulator") or x86 virtual environment and interfaces with a simulation. This allows robot code to run on users' computers as a normal application and to communicates with Synthesis. Emulating this low layer of robot code runtime enables compatibility across new releases of WPILib and other external solutions. Read more [here](./hel/README.md "HEL README").

#### Engine and Exporter Integration

Although HEL may support emulation of a given input or output, it must interface with the engine simulation, which uses the user program to control the robot model and emulate RoboRIO inputs. This robot model was generated using Synthesis's [robot exporter tool](../exporters/robot_exporter "Robot Exporter Source"). In terms of emulation, the robot exporter tool is used to specify which inputs and outputs are attached to which parts of the robot. This means that as features are added to HEL, they must must also be added to the engine and robot exporter. 

# Emulation Bridge and API

HEL communicates with the engine using [gRPC](https://grpc.io/ "gRPC Home Page"), an RPC framework utilizing [Google Protocol Buffers](https://developers.google.com/protocol-buffers/ "Google Protocol Buffers"). The definition of this API is located in the `api/v1` directory. HEL's CMake script automatically builds its own gRPC interface, while the C# solution in `EmulationCommunication` builds the interface used by the engine. The gRPC bridge application located in the `bridge` directory routes the connection from the engine to the correct virtual machine (x86 for Java; ARM for C++) and its running HEL instance.

# x86 Cross-Compilation

Due to recent updates to [WPILib](https://github.com/wpilibsuite/allwpilib "WPILib Source"), a separate virtual machine using an x86 architecture is now required to emulate Java and other JVM user programs. [Docker](https://www.docker.com/ "Docker Home Page") is used with [Buildroot](https://buildroot.org/ "Buildroot Home Page") to generate this x86 virtual machine. To compile WPILib's hardware abstraction layer (the HAL Athena implementation) for x86 instead of ARM, two shim files were needed, which are located in the `wpi_cross` folder.