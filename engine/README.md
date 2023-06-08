# Synthesis Engine
This is the main Simulator aspect to Synthesis. We take the robots and fields that have been exported using our exporter, and simulate them with realtime physics within Unity.

# Building

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
4. Either click the play button above to play within the Unity Editor or locate the build menu under `File -> Build Settings...`
