# Synthesis Engine
This is the main Simulator aspect to Synthesis. We take the robots and fields that have been exported using our exporter, and simulate them with realtime physics within Unity.

# Building
### Requirements
- Unity 2020.3.12f1
## Prerequisites
### Engine Importer & Simulator API
1. Navigate to the [README](/importer/README.md) located inside of the [importer](/importer/) directory and follow the Building and Post Build steps.
## Compiling
1. Open the [engine](/engine/) directory with Unity.
2. Navigate to the top bar and open the NuGet package manager by going to `NuGet -> Manage NuGet Packages`
3. Use the package manager to install the following packages:
    1. MathNet.Numerics
    2. MathNet.Spatial
    3. Google.Protobuf
4. Navigate inside Unity to the `Assets/Scenes` directory.
5. Double-click `MainScene` to open the main scene inside of Unity.
6. Either click the play button above to play within the Unity Editor or locate the build menu under `File -> Build Settings...`