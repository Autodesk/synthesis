# Synthesis Engine
This is the main Simulator aspect to Synthesis. We take the robots and fields that have been exported using our exporter, and simulate them with realtime physics within Unity.

# Building
### Requirements
- Unity 2020.3.12f1
## Prerequisites
### Synthesis API
1. Navigate to the [API README](/api/README.md) and follow the build instructions.
2. Run the `post_build` script to copy the built binaries into engine.
    - For Windows: `$ post_build.bat`
    - For Linux/Macos: `$ ./post_build.sh`
## Compiling
1. Navigate to [EngineDeps](/engine/EngineDeps/), and follow the [README](/engine/EngineDeps/README.md) to install the dependencies.
2. Open the [engine](/engine/) directory with Unity.
    - It will likely ask whether or not you wish to enter into safe mode. Just click ignore.
3. Navigate inside Unity to the `Assets/Scenes` directory.
4. Double-click `MainScene` to open the main scene inside of Unity.
5. Either click the play button above to play within the Unity Editor or locate the build menu under `File -> Build Settings...`
