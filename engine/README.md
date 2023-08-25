# Synthesis Engine

[![Engine](https://github.com/Autodesk/synthesis/actions/workflows/Engine.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/Engine.yml)

This is the main Simulator aspect to Synthesis. We take the robots and fields that have been exported using our exporter and simulate them with realtime physics within Unity.

## Getting Started

For more information on cloning this repository and the initial setup of the Synthesis codebase please visit the [Getting Started](/README.md#getting-started) section of the root *README*.

### Dependencies

Similar to the rest of Synthesis, the Synthesis Engine has the following dependencies that need to be satisfied before attempting to build Synthesis or the Synthesis Engine. Differently from the rest of Synthesis however, the Synthesis Engine requires other parts of Synthesis to be built before and linked. For this reason we strongly recommend using an `init` script to setup and build the Synthesis Engine dependencies. There is of course the option of manually building these dependencies, but please please don't do this.

- External Dependencies:
  - [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0)
  - [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
  - [Protobuf 23.3](https://github.com/protocolbuffers/protobuf/releases/tag/v23.3)
  - Unity Version 2022.3.2f1 (d74737c6db50)
- Internal Dependencies:
  - [Synthesis API](/api)
  - [Protocols](/protocols/)
  - [EngineDeps](/engine/EngineDeps/)

For each of these internal dependencies, please follow the instructions in their respective *README*s. Alternatively, you can run the included `init` script for your operating system to do this automatically.

### Building The Synthesis Engine With Unity

Before attempting to build the Synthesis Engine, ensure you have all dependencies installed.

1. Open `synthesis/engine` in Unity.
2. From there, you can run the simulation engine inside the Unity editor by opening `MainScene` from the `Assets/Scenes` directory or build it as a standalone application.
    - To build Synthesis as a standalone application, go to `File -> Build Settings` and select your target platform. Then, click `Build` and select a location to save the built application.
