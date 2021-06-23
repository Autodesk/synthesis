# Importer
This contains the PTL (Protobuf Transition Layer) as well as all the backend code for converting serialized data into GameObjects inside the Engine
### PTL (Protobuf Transition Layer)
The PTL has designed to provide support for legacy exported data such as the BXDF and BXDJ formats.

# Building
### Requirements
- .NET Standard 2.0 (Required)
- .NET Core 3.1 (Required for unit tests)
- Visual Studio 2019 (Recommended)
## Prerequisites
### SimulatorAPI
1. Navigate to the [aardvark/SimulatorFileIO](/aardvark/SimulatorFileIO/) directory inside the repository.
2. Open the [SimulatorAPI.sln](/aardvark/SimulatorFileIO/SimulatorAPI.sln) file with Visual Studio.
3. Build the solution from the Visual Studio toolbar.
## Compiling
1. Navigate to the [importer](/importer/) directory inside the repository.
2. Open the [Importer.sln](/importer/Importer.sln) file with Visual Studio.
3. Build the solution from the Visual Studio toolbar.

## Post Build
To add the [EngineImporter.dll](/engine/Assets/Packages/EngineImporter.dll) to the engine, run [post_build.bat](/importer/post_build.bat) located inside of the [importer](/importer/) directory.