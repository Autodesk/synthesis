#QEMU RoboRIO Emulator
## Overview
National Instrument's RoboRIO runs on a Xilinx Zynq A9 CPU and chipset, which QEMU, the Quick EMUlator, happens to have built in support for.

## Build
### Dependencies
7-Zip: extracting files

### Disk image
At this time, there are no scripts for building the RoboRIO disk image in Windows. To build the image in Linux, check out [this repository](https://github.com/robotpy/roborio-vm). After you have a disk image, drop it in the same folder as this readme.

### Everything else
For the rest of the initialization, `initialize.bat` should do the trick if you're okay with using a stock kernel.If, for whatever reason, you're not okay with the stock kernel, drop a `uImage` and a `devicetree.dtb` inside a `linux` directory and run `qemu.bat`.

## Usage
### Starting
Running `start.bat` after following the steps in the build section should do the trick. Should you need to reset the disk image to it's initial state, `reset.bat` is there for you.

### Logging in
After the line saying something like `Welcome to LabVIEW`, type `admin` and hit enter twice. (The password is blank)

### Powering off
Either `poweroff` or `shutdown -h now` will do the trick. You can close the console window (disconnect power to the VM) after the line `reboot: System halted`.