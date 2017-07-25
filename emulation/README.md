# Emulation
## Overview
Here you'll find things regarding emulating hardware, firmware, and/or software, usually related to the RoboRIO.
there are serval projects here.

### fake-wpilib 
fake-wpilib is the oldest project for running code on the simulator.
it is combination of partially edited wpilib code and a write of 
fakefpga and network code.

##### warning
this code is highly out of date. badly written and not reviewed.
this is decrapcated project.


### fake-Hal 
#### old research development.
fake-Hal is an attempt to rewrite the hal section of the official wpilib to allow  
the simulator to understand commands from code. this would be alot simpler and avoid
the need for any fpga simulatation than fake wpilib.




### Emulator
#### official project for emulation
this is the newest attempt at getting the code emulatation working. 
it makes the code run in a "virtual machine".
this code relies on the xliynx qemu project for emulation. 

the reason for this is it will allow teams to update the wpilib without updating  
emulatation software. ideally developing against qemu will be signaficantly less painful for both
syntheis developer and robot developers.
