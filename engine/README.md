# Synthesis Engine

[![Engine](https://github.com/Autodesk/synthesis/actions/workflows/Engine.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/Engine.yml)

This is the main Simulator aspect to Synthesis. We take the robots and fields that have been exported using our exporter, and simulate them with realtime physics within Unity.

## Getting Started

For more information on cloning this repository and the initial setup of the Synthesis codebase please visit the [Getting Started](../README.md#getting-started) section of the root *README*.

### Dependencies

Similar to the rest of Synthesis, the Synthesis Engine has the following dependencies that need to be satisfied before attempting to build Synthesis or the Synthesis Engine. Differently from the rest of Synthesis however, the Synthesis Engine requires other parts of Synthesis to be built before and linked. For this reason we strongly recommend using an `init` script to setup and build the Synthesis Engine dependencies. There is of course the option of manually building these dependencies, but please please don't do this.

- External Dependencies:
  - [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0)
  - [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
  - [Protobuf 23.3](https://github.com/protocolbuffers/protobuf/releases/tag/v23.3)
  - Unity Version 2022.3.2f1 (d74737c6db50)
- Internal Dependencies:
  - [Synthesis API](/../api/)
  - [Protocols](/../protocols/)

<!-- # Building

You can either follow the setup or run the `init` scripts at the [root](/) of the repository:
- For Windows: `$ init.bat`
- For Linux/Macos: `$ ./init.sh`

### Requirements
- [Unity 2021.3.9f1](https://unity.com/releases/editor/whats-new/2021.3.9)
- [.NET](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
    - I suggest .NET 6, though many compile .NET Standard Libs
- [Protobuf Compiler v3.19.4](https://github.com/protocolbuffers/protobuf/releases/tag/v3.19.4)
## Prerequisites
### Synthesis API
1. Navigate to the [API README](/api/README.md) and follow the build instructions.
2. Run the `post_build` script to copy the built binaries into engine.
    - For Windows: `$ post_build.bat`
    - For Linux/Macos: `$ ./post_build.sh`
### Nuget Dependencies
1. Navigate to [EngineDeps](/engine/EngineDeps/) and follow the [README](/engine/EngineDeps/README.md) to install the dependencies.
## Compiling
1. Open the [engine](/engine/) directory with Unity.
    - It will likely ask whether or not you wish to enter into safe mode. Just click ignore.
2. Navigate inside Unity to the `Assets/Scenes` directory.
3. Double-click `MainScene` to open the main scene inside of Unity.
4. Either click the play button above to play within the Unity Editor or locate the build menu under `File -> Build Settings...` -->
