# Emulation
## Overview
Here you'll find things regarding emulating hardware, firmware, and/or software, usually related to the RoboRIO.

### Hardware Emulation Layer

`hel` is (mainly) a reimplementation of the hardware abstraction layer that compiles and runs natively on Windows which allows your robot code to do the same and to communicate with Synthesis. By reimplementing a lower level of the robot code runtime, we can improve maintainability and portability of code across new releases of WPILib. This is currently in development and will replace the old solution soon.

### HEL Build Tool

`HELBuildTool` is for compiling robot code via Makefile outside of Eclipse targeted at Synthesis, so that your code can run natively on Windows and communicate with the simulator. In the future, this will be integrated into Eclipse to make the compilation process more intuitive and easier to use.
