# Hardware Emulation Layer

## How It Works

HEL is a re-implementation of the Ni FPGA which would normally run on the RoboRIO. FRC user code interfaces with WPILib, which is a high level library built on HAL (the RoboRIO's hardware abstration layer). In turn, HAL is built on the Ni FPGA, which interfaces with hardware. Ni FPGA code is available as a set of header files, which can be found in allwpilib under [ni-libraries](https://github.com/wpilibsuite/allwpilib/ "allwpilib"). These headers contain pure abstract classes which HEL implements using derived classes. So where HAL calls Ni FPGA functions which normally communicate with hardware, those calls instead use HEL's implementation which communicate with its core, a `RoboRIO` Singleton instance which handles the data. 

Reading into that `RoboRIO` instance, background threads serialize and deserialize data as JSON to communicate with Synthesis's engine over TCP. They update `RoboRIO` with received data such as joystick and encoder inputs while transmitting outputs such as PWM signals to the simulated robot. 

## Scope of Emulation and Simulation

Since HEL is a re-implementation of  the Ni FPGA, it has the potential to support all RoboRIO inputs and outputs, including network data from the FRC Driver Station such as alliance station ID. Currently, HEL and the engine support: 
* Talon SRX and Victor SPX CAN outputs
* PWM header outputs to motor controllers
* Gamepad inputs
* Encoder inputs (drive train encoders only)
* Match information 
	* Robot mode
	* Alliance station ID
	* Enabling/disabling
	* Game specific messages

In the future, HEL and the engine will be expanded to support more features.

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

After the utilities have been installed, all that is left to do is build. To get started and build HEL, navigate to the `hel` directory in a terminal and execute the following command:

```shell
cmake . -DCMAKE_BUILD_TYPE=RELEASE \
        -DARCH=ARM;
make hel;
```

The target architecture can be specified using `-DARCH=(ARM|X86)`. The build mode can be specified using `-DCMAKE_BUILD_MODE=(RELEASE|DEBUG)` to enable or disable debug symbols. To build tests, specify `-DTESTING=(ON|OFF)`; note that HAL-, CTRE-, and WPILib-based tests are not supported in x86 mode. If building for x86, benchmarks can be built with `-DBENCHMARKS=(ON|OFF)`. Doxygen comments can be built with `-DBUILD_DOC=(ON|OFF)`.

## Project Structure

#### benchmarks
This directory contains all of the Google Benchmark benchmarking files for HEL.

#### docs
This directory contains a Doxygen Doxyfile and acts as the target for the documentation Doxygen generates.

#### external-configs
This directory contains configurations for external to modify their build processes for compatibility with HEL and each other. 

#### include
This directory contains all of HEL's header files.

#### lib
This directory contains all of the downloaded external projects. When they and HEL are built, the resulting libraries are stored here as well.

#### scripts
This directory contains useful bash scripts used in the build process and for ease of use during development.

#### src
This directory contains all of HEL's cpp files

#### tests
This directory contains all of the Google Test testing files for HEL.
