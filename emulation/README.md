# Emulation
## Overview
Here you'll find things regarding emulating hardware, firmware, and/or software, usually related to the RoboRIO.

### Hardware Emulation Layer
`hel` is a reimplementation of the hardware abstraction layer that runs in an ARM virtual environment (configured akin to the RoboRIO), which allows your robot code communicate with Synthesis. By reimplementing a lower level of the robot code runtime, we can improve maintainability and portability of code across new releases of WPILib and other external solutions. This is new system has been integrated into Synthesis, replacing old version of the emulator (HELBuildTool and prior solutions).

# Building
The implementation of HEL currently relies on emulating code that the robot code requires several levels down. For the easiest user experience, the code is all handled inside of a virtual machine running Linux that emulates an ARM proccessor. The problem there is that
in the case of most users, this is neither the operating system you are running nor the processor your computer has. For ease of development, our system is built around Linux as the development environment. We recommend looking into a free virtual machine solution 
(there are many good ones out there) and a linux operating system (Ubuntu is recommended). There are many tutorials on setting up virtual machines, so it is recommended to follow one of them. Once you have your virtual machine set up, you need to install several 
pieces of software. The first of those is the build system CMake. On Ubuntu, the commands are as follows:

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

After all of the utilities are installed, all thats left to do is build. To build HEL, execute the following code in a terminal on the virtual machine:

```shell
cmake . -DCMAKE_BUILD_TYPE=RELEASE \
        -DARCH=ARM;
make hel;

```

