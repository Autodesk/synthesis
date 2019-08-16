
# Hardware Emulation Layer

HEL is the core of Synthesis's emulation, redirecting robot code hardware communication to the Synthesis simulator. 

* [How It Works](#how-it-works)
* [Scope of Emulation and Simulation](#scope-of-emulation-and-simulation)
* [Building HEL](#building-hel)
* [Testing HEL](#testing-hel)
* [Project Structure](#project-structure)

## How It Works

HEL is emulation of a layer of robot code several levels below that at which users develops. For the easiest user experience, the C++ code is handled inside of a Linux virtual machine emulating an ARM processor, much akin to the environment that runs on a RoboRIO. Java and other JVM code runs in a separate x86 Linux virtual machine.

HEL is a re-implementation of the NI FPGA, which normally runs on the RoboRIO. FRC user code interfaces with WPILib, which is a high-level library built on HAL, the RoboRIO's hardware abstraction layer. In turn, HAL is built on the NI FPGA, which interfaces with hardware. NI FPGA code is available as a set of header files, which can be found on GitHub [here](https://github.com/wpilibsuite/ni-libraries). These headers contain pure abstract classes which HEL re-implements using derived classes. Where HAL would calls NI FPGA functions to communicate with hardware, in Synthesis, those calls use HEL functions which to manage emulated RoboRIO state in a Singleton instance. 

The user program calls functions to set this emulated RoboRIO's outputs, which are then transmitted to the Synthesis engine using [gRPC](https://grpc.io/ "gRPC Home Page"). The engine responds with simulated input values, which are likewise mapped into the emulated RoboRIO. 

## Scope of Emulation and Simulation

| Library            | Supported Version |
|--------------------|-------------------|
| WPILib (C++; Java) | v2019.4.1         |
| NI Libraries       | v2019-12          |

Since HEL is a re-implementation of the NI FPGA, it has the potential to support all RoboRIO inputs and outputs, including network data from the FRC Driver Station such as alliance station ID. Currently, HEL and the engine support: 
* Talon SRX, Victor SPX, and Spark MAX CAN motor controller outputs
* PWM header and MXP outputs to motor controllers
* Gamepad inputs
* Encoder inputs (drive train encoders only)
* Match information 
	* Robot mode
	* Alliance station ID
	* Enabling/disabling
	* Game specific messages

In addition, users can view outputs and edit inputs for the following I/O in the Emulation Robot IO Panel:
* Digital headers and MXP inputs and outputs
* Analog headers and MXP inputs and outputs
* Relays
* The RoboRIO user button 

This panel also gives them access to their user program print-outs.

In the future, HEL and the engine will be expanded to support more features.

## Building HEL

For ease of development, the development environment is built around the same operating system as the emulator: Linux. For those seeking to contribute to the Synthesis emulator, it is recommended to either install Linux or look into running Linux on a virtual machine solution with their current system (Ubuntu is recommended). Once the Linux environment is set up, some software must be installed. The first of those is the build system CMake. To install on Ubuntu, the commands are as follows:

```shell
sudo apt update
sudo apt-get install cmake;

# For a better CMake experience, a GUI can be installed
sudo apt-get install cmake-gui;

# Necessary only for Java users
sudo apt install openjdk-11-jre-headless;
sudo apt install openjdk-11-jdk;

# Necessary for all users
sudo apt-add-repository ppa:wpilib/toolchain
sudo apt-add-repository ppa:wpilib/toolchain-beta
sudo apt update 
sudo apt install frc-toolchain
sudo apt install frc2019-toolchain
```

Now HEL can be built. Navigate to the `hel` directory in a terminal and execute the following command:

```shell
mkdir -p build && cd build
cmake ..
make hel
```
### CMake Flags
| Flag               | Arguments                                     | Description                                                  |
|--------------------|-----------------------------------------------|--------------------------------------------------------------|
| -DBENCH            | ON or OFF                                     | Enable benchmarks                                            |
| -DBUILD_DOC        | ON or OFF                                     | Enable Doxygen documentation (Builds by default for release) |
| -DCMAKE_BUILD_TYPE | Release, Debug, MinSizeRel, or RelWithDebInfo | Select build type (see table below)                          |
| -DNO_ROBOT         | ON or OFF                                     | Disable test user programs                                   |
| -DNO_TESTING       | ON or OFF                                     | Disable unit tests                                           |
| -DX86              | ON or OFF                                     | Cross-compile for x86 (ARM by default)                       |
| -DGRPC_VERSION     | [version] (e.g. v1.21.1)                      | Specify gRPC version                                         |
| -DNILIB_VERSION    | [version] (e.g. v2019-12)                     | Specify NI library version                                   |
| -DWPILIB_VERSION   | [version] (e.g. v2019.4.1)                    | Specify WPILib version                                       

Note that HAL-, CTRE-, and WPILib-based tests are not supported in x86 mode.

### Build Types
| Build Type     | Description                                        |
|----------------|----------------------------------------------------|
| Release        | Build with optimizations and Werror                |
| Debug          | Build with debug symbols and disable optimizations |
| MinSizeRel     | Build with optimizations for size                  |
| RelWithDebInfo | Build with medium optimizations with debug symbols |


The project can be cleaned using the clean script:

```shell
./clean.sh (all|cmake|emulator|gtest|wpilib|ctre|asio|hel|user-code|docs)
```

## Testing HEL

When built for x86 architecture, HEL's Google Test and Benchmark files can be run natively; though tests will not be able to use WPILib, HAL, or CTRE code. They build to `bin/tests` and `bin/benchmarks` respectively. To test HEL in the emulation environment, either the release emulator or a developer emulator built from scratch (see [here](./../emulator_building.md "emulator_building.md")) must be used, and the test files, test projects, user code, and libhel, deployed. To do so, run the following commands:

```shell
./run_vm.sh                                      # Download and run the release emulator

./scripts/deploy.sh                              # Deploys libhel, user code, and tests automatically
./scripts/deploy.sh build/tests/file_name        # Or deploy specified files
./scripts/login.sh                               # SSH and sign into the emulator instance

# Running user code and tests from the emulator
./frc_program_runner.sh                          # Runs user code
./tests/test_name                                # Run a given test

# Testing outside of the emulator
./build/tests/test_name                          # Run a given test
./build/bechmarks/benchmark_name                 # Run a given benchmark
./scripts/receieve_data.sh                       # Receive data running user code on emulator sends to engine
```

## Project Structure

#### benchmarks
This directory contains all of the Google Benchmark benchmarking files for HEL.

#### docs
This directory contains a Doxygen Doxyfile and acts as the target for the documentation Doxygen generates.

#### external_configs
This directory contains configurations for external to modify their build processes for compatibility with HEL and each other. 

#### include
This directory contains all of HEL's header files.

#### build
This directory contains all of the downloaded external projects. When they and HEL are built, the resulting libraries are stored here as well.

#### scripts
This directory contains useful bash scripts used in the build process and for ease of use during development.

#### src
This directory contains all of HEL's cpp files.

#### tests
This directory contains all of the Google Test testing files for HEL.