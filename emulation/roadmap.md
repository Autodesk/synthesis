# Hardware Emulation Layer Roadmap and Future Goals

- [Code Cleanup](#code-cleanup)
- [Eclipse Plugin](#eclipse-plugin)
- [Command Line Tools](#command-line-tools)
- [Sensor Support](#sensor-support)
- [Long Term](#long-term)
  - [Unit Testing and Offline Autonomous](#unit-testing-and-offline-autonomous)
  - [Camera Support and OpenCV](#camera-support-and-opencv)
  - [Support for Other Languages](#support-for-other-languages)

## Code Cleanup

One of the major goals for the future should be to clean up the existing code, especially in the parts for communication with the driver station and the simulator. There are a two major issues that stand out.

Stylistically, the entire codebase is pretty inconsistent. We probably want to figure out a standard for how we want everything to be styled, but we also want it to match the existing parts of the HAL. This means that things like two-space indents are probably going to stay in. Unfortunately, some of the code that was added to the HAL uses tabs instead of spaces, which is inconsistent with the existing code and should probably be fixed. The end goal with this would probably be to get an autoformatter like [clang-format](https://clang.llvm.org/docs/ClangFormat.html) working. This should be available under the `clang` package in cygwin, and could be added to the makefile. This is, of course, not a requirement but it would be very nice to have if somebody wants to put in the effort up front to configure it.

The code for communication with the driver station (`hel/lib/athena/FRCDriverStation.cpp` and `hel/lib/athena/DriverStationInternal.h`) is pretty bad and we should probably just rewrite the whole thing. It is very inconsistent with the other sockets code because it is written in (bad) C style, so if we are going to keep it we should probably at least refactor it to be more C++ style. We should also definitely get rid of the OSAL dependency and replace it with just STL stuff because the only thing that's being used from OSAL is the semaphore implementation, which is available in STL. In addition, we should replace the semaphore use in this segment with just a mutex because there is no reason to use a semaphore here.

The code for communication with the simulator (`hel/lib/StateNetwork/...`) is quite a bit better, but it could still use a lot of refactoring and possibly a rewrite. First, as mentioned above, it is written using tabs instead of spaces, which doesn't match the rest of the codebase. It also has a dependency on OSAL for no apparent reason. The code for dealing with the threading (`StartUnityThread` and `StateNetworkThread` in `StateNetworkServer.cpp`) are a little ugly right now and should probably be moved into static member functions of the `StateNetworkServer` class. `pwmValues` should be moved into a member variable of `StateNetworkServer` and we should create a method to access it through a mutex instead of the current system (in which it is unprotected).

In general, although we have commented out most of the functionality from the HAL, there is still a lot of code left which could be removed. We left most things in as long as they didn't actually touch the hardware, but a lot of that isn't useful for us now so it could be deleted. For example, there is a lot of stuff going on in `PWM.cpp` involving converting to a raw value from a speed, but since we don't ever use the raw value this should be removed.

## Eclipse Plugin

The current version of the HEL relies on `HELBuildTool` to build the code. `HELBuildTool` is just a simple graphical wrapper over some makefiles, and it's fairly tedious to use repeatedly, especially for ~~inferior languages~~ java. Unfortunately, the most obvious way to improve this for the majority of end users is an eclipse plugin that integrates with the existing workflow. Eclipse has some (very bad) makefile support, so that could probably be leveraged to avoid too much code duplication. Either way, this is going to be highly upleasant, but probably worth it in the long run.

## Command Line Tools

In addition to an eclipse plugins, many developers are comfortable on a command line, so creating command line build tools would be a vast improvement for a fraction of our user base who is familiar with using the command line. This should probably be implemented in such a way that people could use it from both cygwin and cmd to include the most people. Overall, I think this would be helpful for less people than doing an eclipse plugin, but it would be much easier and be a really nice feature for people who know how to use it.

## Sensor Support

Sensors are one of the most requested features by programmers, so we should probably add them pretty soon. Setting up sensors is going to require some work up front in both the simulator and emulator to make the `StateNetwork` communication stuff double ended. Currently, the user code can only really send data from the simulator, not receive it. This shouldn't be too hard, especially if the `StateNetwork` code is cleaned up, but it will require work on both the simulator and emulator. After that, we will need to figure out what level we should be sending the sensor data at. One possibliity is to configure which sensor types you are using and what signal ranges they expect in the simulator or exporter and then just send the raw data over UDP. Another option (which is better in my opinion), is to find a way to figure that out through the way the HAL is being used in user code. This may require modifying wpilib a little. Probably the first step is going to be reading the code on the `allwpilib1` repository for this sensor, because most of the implementation code is not in our copy right now.

## Long Term

Here are a few ideas that I had for farther in the future that might be useful to implement.

### Unit Testing and Offline Autonmous

One really nice feature would be the ability to run your autonomous code 50,000 times really fast as part of a unit testing system. This would probably be headless, so we wouldn't need to worry about graphics, only the simulation speed. This would probably require a huge effort to set up the simulator for headless usage, but it would be really useful.

### Camera Support and OpenCV

Currently, all the code referring to cameras or OpenCV is commented out. This is because OpenCV is a huge dependency and we don't have good support for streaming the camera to the robot code anyway. Eventually, we are probably going to want to bring OpenCV back in and add support for both normal USB cameras and the camera from the simulator. It might be worth looking into OpenCV's [`VideoCapture`](http://docs.opencv.org/2.4/modules/highgui/doc/reading_and_writing_images_and_video.html#videocapture) for this purpose. The `VideoCapture` would allow us to hook into a normal stream from `localhost` as long as we set this up on the simulator side and would be an easy way to add support for this. Either way we are most likely going to have to modify `cscore`.

### Support for Other Languages

Not all teams use C++ or ~~the inferior language~~ java. The most popular alternative is Python (we don't speak of LabView here). This should be relatively easy, but it's fairly low priority and requires mostly makefile related work. If somebody working on the emulator is familiar with makefiles, it should probably only take a few hours to add this. Basically, all you need is to build the HAL as a dynamic library instead of static and then hook into the `libHALAthena.so` loading code with `LD_PRELOAD`.
