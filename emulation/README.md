# FRC Robot Code Emulation

## Overview
The emulator is the Synthesis tool designed to help users test their FRC robot code. Users can upload their own user code as the FRCUserProgram they would normally deploy to the RoboRIO. Its communication with hardware will then be redirected to the engine for simulation and testing.

## Hardware Emulation Layer
The core of Synthesis's emulator is HEL. HEL is a reimplementation of the Ni FPGA which runs on the RoboRIO which runs in an ARM virtual environment rather than the RoboRIO and interfaces with a simulation rather than hardware. This allows robot code to run on users' computers and to communicate with Synthesis. This lower layer of robot code runtime was chosen for re-implementation to enable compatibilty across new releases of WPILib and other external solutions. This is a new system that has been integrated into Synthesis, replacing older versions of the emulator such as the HELBuildTool and prior solutions.

## Building
HEL is emulation of a layer of robot code several levels below that at which users develops. For the easiest user experience, the code is all handled inside of a Linux virtual machine emulating an ARM processor, much akin to the environment that runs on a RoboRIO. For ease of development, the development environment is built around the same operating system, Linux, as the emulator. It is recommended for those seeking to develop emulation to either install Linux or look into running Linux on a virtual machine solution with their current system (Ubuntu is recommended). Once the Linux environment is set up, there are a few pieces of software to install. The first of those is the build system CMake. To install on Ubuntu, the commands are as follows:

```shell
sudo apt update && sudo apt-get install cmake;

# Necessary only for Java users
sudo apt install openjdk-8-jre-headless;
sudo apt install openjdk-8-jdk;

# Necessary for all users
sudo apt-add-repository ppa:wpilib/toolchain
sudo apt update 
sudo apt install frc-toolchain

```

After the utilities have been installed, all that is left to do is build. To build HEL, navigate to the `hel` directory in a terminal and execute the following command:

```shell
cmake . -DCMAKE_BUILD_TYPE=RELEASE \
        -DARCH=ARM;
make hel;

```

