# Emulation
## Overview
Here you'll find things regarding emulating hardware, firmware, and/or software, usually related to the RoboRIO.

### WPILib Reimplementation 
`fake_wpilib` is the solution currently used for running code in the simulator. It is combination of a rewritten WPILib, network code, and a fake FPGA.


### Emulator
`emulator` was an attempt at virtualizing the RoboRIO such that the Synthesis maintainers didn't have to touch WPILib code at all, and instead modify the hardware abstraction layer such that robot controlling functions would talk to Synthesis instead of to a physical RoboRIO. A few issues were had here, however, so we opted for a reimplementation of the hardware abstraction layer instead.

### Hardware Emulation Layer

`hel` is (mainly) a reimplementation of the hardware abstraction layer that compiles and runs natively on Windows which allows your robot code to do the same and to communicate with Synthesis. By reimplementing a lower level of the robot code runtime, we can improve maintainability and portability of code across new releases of WPILib. This is currently in development and will replace the old solution soon.