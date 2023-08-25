# Synthesis API

[![API](https://github.com/Autodesk/synthesis/actions/workflows/API.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/API.yml)

The Synthesis API contains parts of Synthesis that can remain mostly Unity abnostic. The end goal of this API is to be used to extend Synthesis'
functionality and reused as throughout future iterations of Synthesis no matter where it may go.

## Getting Started

For more information on cloning this repository and the initial setup of the Synthesis codebase please visit the [Getting Started](/README.md#getting-started) section of the root *README*.

### Dependencies

Similar to the rest of Synthesis, the Synthesis API has the following dependencies that need to be satisfied before attempting to build Synthesis or the Synthesis API.

- [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-1-0)
- [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Protobuf 23.3](https://github.com/protocolbuffers/protobuf/releases/tag/v23.3)
- [Visual Studio 2017](https://visualstudio.microsoft.com/downloads/) or newer (recommended)

You can either install these dependencies manually or use the *resolve dependencies* scripts (`scripts/win/resolve_deps.bat` & `scripts/osx/resolve_deps.sh` respectively) to install some of them for you.

To automatically install .NET and Protobuf run the following script depending on your operating system:
- Windows: `scripts/win/resolve_deps.bat`
- MacOS & Linux: `scripts/osx/resolve_deps.sh`

Note that the windows script requires admin privileges to install .NET and Protobuf to the system PATH. For this reason it is not included by default in `init.bat` unlike how `init.sh` includes `resolve_deps.sh`.

### How To Build The API With An `init` Script

Synthesis comes with a robust build system that includes an `init` script that will automatically compile and link each part of Synthesis together. This is the recommended way to build Synthesis and the Synthesis API.

To build the API with an `init` script first ensure that all dependencies are resolved. Then run the following command based on your operating system in the [root](../) of this repository:

- Windows: `init.bat`
- MacOS & Linux: `init.sh`

### How To Build The API With The API Build Scripts

The Synthesis API comes with it's own build scripts to simplify the process of building the API from the command line. First ensure that all dependencies are resolved. Then run the following command based on your operating system in the [api](/api/) directory of this repository:

- Windows: `build.bat`
- MacOS & Linux: `build.sh`

### How To Build The API With Visual Studio

1. Open Visual Studio 2017 or newer.
2. Select `Open Project or Solution`.
3. Choose `api.sln` from the [api](/api/) directory of this repository.
    - For more information about cloning this repository please visit the [Getting Started](/README.md#getting-started) section of the root *README*.
4. Select `Build Solution` from the Visual Studio toolbar or hit `Ctrl + Shift + B` to build the API. 
